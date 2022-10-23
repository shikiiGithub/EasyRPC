#define SOCKERTTEST
using shikii.Hub.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
 
namespace shikii
{
    namespace Hub
    {
        namespace Networking
        {
            /// <summary>
            /// 消息路由
            /// </summary>
            /// <param name="nWhichClient">客户端索引（对于客户端第一个参数请填写 ""）</param>
            /// <param name="msgKind">消息类型</param>
            /// <param name="data">包含消息头的 buffer</param>
            public delegate void RouteCallback(string clientId, byte msgKind, byte[] data);
            public class TCPServer : TCPBase
            {
                public bool booted = false;
                public delegate void ClientConnectedCallback(int nIndex);
                public delegate void ClientDisconnectedCallback(String ClientInfo);
                public event ClientConnectedCallback ClientConnected;
                public event ClientDisconnectedCallback ClientDisconnected;

                public event RouteCallback Route;
                //Each SubLoop Thread Content Buffer To Recieve Client Message

                protected List<bool> lst_Thd_Ctrls;
                public bool MainLoopCtrl = true;

                // Deal More Clients Threads
                protected List<Thread> lstThdArr_SubContent;
                // Record Connected Client IDs ;
                public List<String> lstStrArr_ClientID;
                protected List<Socket> lst_Clients;

                protected Socket ServerSocket;
                public List<Socket> SocketClients
                {

                    get { return lst_Clients; }
                }

             

                public TCPServer()
                {

                    DefaultConfig();
                    InitCollections();
                }
                bool Boot(String strIP)
                {
                    try
                    {
                        this.bEndNetwork = false;
                        ServerIP = IPAddress.Parse(strIP);
                        IPEndPoint ServerEndPoint = new IPEndPoint(ServerIP, nPort);

                        ServerSocket =
                        new Socket(
                           AddressFamily.InterNetwork,
                           SocketType.Stream, ProtocolType.IP);
                        ServerSocket.ReceiveBufferSize = ServerSocket.SendBufferSize = this.BufferSize;
                        ServerSocket.Bind(ServerEndPoint);
                        ServerSocket.Listen(3);
                        thd_Main = new Thread(Loop);
                        thd_Main.Start();
                        booted = true;
                        Console.WriteLine(String.Format("服务器：{0}:{1}已经启动！", ServerIP, nPort));
                        return true;
                    }
                    catch (System.Exception ex)
                    {
                        booted = false;
                        this.strErrorInfo = String.Format("未能成功构建TCP服务器，IP或者端口错误: {0}。", ex.Message);
                        ExtensionFuncs.TipError(strErrorInfo);
                        return false;
                    }

                }
                //请确保为IP属性赋值了
                public bool Boot()
                {
                    return Boot(IP);
                }

                private void InitCollections()
                {

                    lst_Clients = new List<Socket>();
                    lstThdArr_SubContent = new List<Thread>();
                    lstStrArr_ClientID = new List<string>();
                    lst_Thd_Ctrls = new List<bool>();

                }
                protected override void Loop()
                {
                    while (true)
                    {

                        try
                        {

                            lst_Clients.Add(ServerSocket.Accept());

                        }
                        catch (System.Exception ex)
                        {
                            Console.WriteLine("At TCPServer.Loop:" + ex.Message);
                            continue;

                        }

                        String clientId = GetClientInfo(lst_Clients[lst_Clients.Count - 1]);
                        if (ClientConnected != null)
                            this.ClientConnected(lst_Clients.Count - 1);
                        else
                            Console.WriteLine("未实现‘ ClientConnected 事件 ’");
                        lst_Thd_Ctrls.Add(true);


                        lstThdArr_SubContent.Add(
                            new Thread(SubLoop));

                        lstThdArr_SubContent[lstThdArr_SubContent.Count - 1].Start(clientId);

                    }
                }
                protected void SubLoop(Object obj)
                {
                    String clientId = (String)obj;
                    while (true)
                    {
                        try
                        {
                            
                            Socket sct = lst_Clients[lstStrArr_ClientID.IndexOf(clientId)];
                            byte[] bytMsgLen = new byte[TCPBase.MARKPOSITION];
                            receiveStat:;
                            //读取数据的长度
                            int nRecievedLen = sct.Receive(bytMsgLen, 0, TCPBase.MARKPOSITION, System.Net.Sockets.SocketFlags.None);
                            if (nRecievedLen == 0)
                                goto receiveStat;

                            int nLen = (int)TCPBase.FetchDataLenByts(bytMsgLen);
                            
                            byte[] buffer = new byte[nLen ];
                             bytMsgLen.CopyTo(buffer,0) ;
                            int nCount = TCPBase.MARKPOSITION;
                            int nTotalLen = (int)nLen ;
                            //然后循环读取，确保没有少读
                            while (true)
                            {
                                if (nCount < nTotalLen)
                                {
                                    nCount += sct.Receive(buffer, nCount, nTotalLen - nCount, System.Net.Sockets.SocketFlags.None);
                                }
                                else
                                    break;
                            }

                            byte byt_MSG_Mark = TCPBase.FetchMSGMark(buffer);
                            if (Route != null)
                                Route(clientId, byt_MSG_Mark, buffer);
                            else
                                Console.WriteLine("未实现自定义消息 Route,自定义消息无法处理");

                        }
                        catch (Exception e)
                        {
                            int nIndex = lstStrArr_ClientID.IndexOf(clientId);
                            Console.WriteLine(e.Message);
                            this.strErrorInfo = e.ToString();

                            if (ClientDisconnected != null)
                                this.ClientDisconnected(lstStrArr_ClientID[nIndex]);
                            else
                                Console.WriteLine("ClientDisconnected 事件未实现");

                            try
                            {
                                lst_Clients[nIndex].Shutdown(SocketShutdown.Both);
                            }
                            catch (System.Exception exx)
                            {

                            }

                            lst_Clients[nIndex].Close();
                            lst_Clients.RemoveAt(nIndex);

                            this.lstStrArr_ClientID.RemoveAt(nIndex);
                            lst_Thd_Ctrls.RemoveAt(nIndex);
                            Thread it = lstThdArr_SubContent[nIndex];
                            lstThdArr_SubContent.RemoveAt(nIndex);
                            int nIndex_NewOne = lst_Clients.Count - 1;
                           
                            return;
                        }

                    }



                }

                public virtual bool Send(String strClientID, byte byt_Msg, byte[] byt_Arr_Content)
                {
                    byte[] bytArr = new byte[TCPBase.MARKPOSITION + byt_Arr_Content.Length];
                    StoreMSGMark(bytArr, byt_Msg);
                    StoreDataLenByts((uint)bytArr.Length, bytArr);
                    byt_Arr_Content.CopyTo(bytArr, TCPBase.MARKPOSITION);
                    Socket sct = GetClientSocket(strClientID);
                    int nResult = sct.Send(bytArr);
                    if (nResult > 0)
                        return true;
                    else
                        return false;

                }

                protected override bool Close()
                {
                    try
                    {
                        this.bEndNetwork = true;

                        // Thread.Sleep(200);

                        try
                        {
                            ServerSocket?.Shutdown(SocketShutdown.Both);
                        }
                        catch (System.Exception ex)
                        {

                            //  ServerSocket.Shutdown(SocketShutdown.Receive);
                        }
                        ServerSocket?.Close();
                        foreach (var item in lst_Clients)
                        {
                            item.Shutdown(SocketShutdown.Both);
                            item.Close();
                        }
                        foreach (var item in lstThdArr_SubContent)
                        {
                            item?.Abort();
                        }
                        thd_Main?.Abort();
                        booted = false;
                        return true;
                    }
                    catch (System.Exception ex)
                    {
                        this.strErrorInfo = ex.ToString();
                        return false;
                    }
                }
                public void Dispose(bool bForceClose = true)
                {
                    Close();
                    if (bForceClose)
                        ForceClose();
                }
                ~TCPServer()
                {
                    Close();
                }

                protected virtual String GetClientInfo(Socket skt)
                {
                    string str = skt.RemoteEndPoint.ToString();
                    this.lstStrArr_ClientID.Add(str);
                    return str;
                }
                public string GetClientIP(int nIndex)
                {
                    return this.lst_Clients[nIndex].RemoteEndPoint.ToString();
                }

                int GetClientIndex(String strClientID)
                {
                    return this.lstStrArr_ClientID.IndexOf(strClientID);
                }

                public Socket GetClientSocket(String strClientID)
                {
                    int nindex =  this.lstStrArr_ClientID.IndexOf(strClientID);
                    return lst_Clients[nindex];
                }
                public void KillClientThread(int nClientIndex)
                {
                    this.lst_Thd_Ctrls[nClientIndex] = false;
                }


            }
        }

     
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;


namespace shikii
{
    namespace Hub
    {

        namespace Networking
        {
            public class TCPClient : TCPBase
            {

                protected Socket Client;
                public bool bIsConnected = false;
                public event RouteCallback Route = null;
                public delegate void OnDisconnectCallback();
               public event OnDisconnectCallback OnDisconnect = null;
                public TCPClient()
                {
                    DefaultConfig();
 

                }
                public bool Connected
                {
                    get
                    {
                        return bIsConnected;
                    }
                }

                //Client ID Is Client IP
                public bool Connect()
                {
                    try
                    {

                        bEndNetwork = false;

                        ServerIP = IPAddress.Parse(IP);
                        IPEndPoint ClientEndPoint =
                            new IPEndPoint(this.ServerIP, nPort);
                        if(Client == null)
                        Client = new Socket(AddressFamily.InterNetwork,SocketType.Stream, ProtocolType.IP);
                        Client.ReceiveBufferSize = Client.SendBufferSize = this.BufferSize;
                        Client.Connect(ClientEndPoint);
                        thd_Main = new Thread(Loop);
                        thd_Main.Start();
                        this.bIsConnected = true;
                        return true;
                    }
                    catch (System.Exception ex)
                    {
                        this.strErrorInfo = ex.ToString();
                      
                        return false;
                    }
                    finally
                    { 
                        this.bIsConnected=false;
                    }

                }

              public  bool Reconnect()
                {
                    try
                    {
                        
                        Socket _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                        bEndNetwork = false;
                        IPEndPoint ClientEndPoint =
                          new IPEndPoint(this.ServerIP, nPort);
                        _client.ReceiveBufferSize = _client.SendBufferSize = this.BufferSize;
                        _client.Connect(ClientEndPoint);
                        Client = _client;
                        thd_Main = new Thread(Loop);
                        thd_Main.Start();
                        this.bIsConnected = true;
                        return true;
                    }
                    catch (System.Exception ex)
                    {
                        this.strErrorInfo = ex.ToString();

                        return false;
                    }
                    finally
                    {
                        this.bIsConnected = false;
                    }
                }

                // Message Loop

                protected void InternalHandleSocketMessage()
                {
                    try
                    { 
                     
                    byte[] messageLenBuffer = new byte[5];
                       
                    uint nRecievedNum = (uint)Client.Receive(messageLenBuffer, 0, TCPBase.MARKPOSITION, System.Net.Sockets.SocketFlags.None);
                    if (nRecievedNum == 0)
                    {
                        bIsConnected = false;
                        return;
                    }
                    int nLen = (int)TCPBase.FetchDataLenByts(messageLenBuffer);
                    byte[] buffer = new byte[nLen];
                    messageLenBuffer.CopyTo(buffer, 0);
                    int nCount = TCPBase.MARKPOSITION;
                    int nTotalLen = (int)nLen;
                    while (true)
                    {
                        if (nCount < nTotalLen)
                        {
                            nCount += Client.Receive(buffer, nCount, nTotalLen - nCount, System.Net.Sockets.SocketFlags.None);
                        }
                        else
                            break;
                    }

                    byte byt_MSG_Mark = buffer[0];

                    if (Route != null)
                        Route("", byt_MSG_Mark, buffer);
                    }
                    catch (Exception e)
                    {
                        this.bIsConnected = false;
                        if (OnDisconnect != null)
                            OnDisconnect();
                        bEndNetwork = true;
                        this.strErrorInfo = e.ToString();
                    }
                }

                protected override void Loop()
                {
                   
                    while (true)
                    {
                        if (bEndNetwork)
                            return; 
                        InternalHandleSocketMessage();
                    }
                    Console.WriteLine("消息接收线程已经退出");
                }

                //Close Socket
                protected override bool Close()
                {
                    try
                    {
                        bEndNetwork = true;
                        thd_Main?.Abort();
                        Client?.Disconnect(false);
                        Client?.Shutdown(SocketShutdown.Both);
                        Client?.Close();
                        this.bIsConnected = false;

                        return true;
                    }
                    catch (System.Exception ex)
                    {
                        this.strErrorInfo = ex.ToString();
                        this.bIsConnected = false;
                        return false;
                    }


                }
                public void Dispose()
                {
                    Close();
                    ForceClose();
                }

                public void Closed()
                {
                    Close();
                }

                public bool Send(byte[] bytArr, byte MSG)
                {
                    byte[] buffer = new byte[bytArr.Length + 5];
                    bytArr.CopyTo(buffer, 5);
                    StoreDataLenByts((uint)buffer.Length, buffer);
                    StoreMSGMark(buffer, MSG);
                    int nNum = this.Client.Send(buffer);
                    if (nNum > 0)
                        return true;
                    else
                        return false;
                }

                public bool Send(String strMsg, byte MSG)
                {
                    try
                    {
                        byte[] bytArr = TextEncode.GetBytes(strMsg);
                        byte[] buffer = new byte[bytArr.Length + 5];
                        bytArr.CopyTo(buffer, TCPBase.MARKPOSITION);
                        StoreDataLenByts((uint)buffer.Length, buffer);
                        StoreMSGMark(buffer, MSG);
                        int nNum = this.Client.Send(buffer);
                        if (nNum > 0)
                            return true;
                        else
                            return false;
                    }
                    catch (Exception ex)
                    {

                        return false;
                    }
                  
                }
                //Get Message Which Contains Words
                public String GetWords(byte[] buffer)
                {
                    uint nRecievedNum = FetchDataLenByts(buffer);
                    return TextEncode.GetString(buffer, TCPBase.MARKPOSITION,
                        (int)nRecievedNum);
                }
                ~TCPClient()
                {
                    Close();
                }
            }
        }


    }
}

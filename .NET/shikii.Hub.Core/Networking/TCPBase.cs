
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
namespace shikii
{
    namespace Hub
    {
        namespace Networking
        {
            public abstract class TCPBase
            {
                public Object Tag = null;
                protected bool bEndNetwork = false;
                protected int nPort = 8040;
                protected String strErrorInfo = null;
                protected IPAddress ServerIP;
                protected Encoding en = Encoding.UTF8;
                private int bufferSize = 8192;
                

                public System.Text.Encoding TextEncode
                {
                    get { return en; }
                    set { en = value; }
                }
                protected Thread thd_Main;
                protected const byte SPLITMARK = 94;
                private String strIP = "127.0.0.1";

                public static byte MARKPOSITION = 5;
                public static void StoreDataLenByts(uint nLen,
                byte[] buffer)
                {
                    BitConverter.GetBytes(
                          nLen).CopyTo(buffer, 1);
                }
                public static uint FetchDataLenByts(byte[] buffer)
                {
                    byte[] byt_Len = new byte[4];
                    for (int i = 1; i < 5; i++)
                    {
                        byt_Len[i - 1] = buffer[i];
                    }
                    uint byteNum = BitConverter.
                            ToUInt32(byt_Len, 0
                           );
                    byt_Len = null;
                    return byteNum;
                }
                public static void StoreMSGMark(byte[] buffer, byte byt)
                {
                    buffer[0] = byt;
                }
                public static byte FetchMSGMark(byte[] buffer)
                {
                    return buffer[0];
                }
                public virtual void DefaultConfig()
                {
                    TextEncode = Encoding.UTF8;
                }
                protected abstract void Loop();
                protected abstract bool Close();
                public IPAddress LoopBack
                {
                    get
                    {
                        return IPAddress.Loopback;
                    }

                }

                [DisplayName("端口号")]
                public int Port
                {
                    get
                    {
                        return this.nPort;
                    }
                    set
                    {
                        this.nPort = value;
                    }
                }

             
                public String LocalIP
                {
                    get
                    {
                       
                        return Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();
                    }
                }


                [DisplayName("IP")]
                public string IP { get { return strIP; } set { strIP = value; } }


                /// <summary>
                /// 缓冲大小（特指TCP/IP 内部的接收/发送的缓存大小）
                /// 以字节为单位,默认 8kb (= 8192)
                /// </summary>
                public int BufferSize { get { return this.bufferSize; } set { this.bufferSize = value; } }

                protected void ForceClose()
                {
                    String strProcessName = System.Diagnostics.Process.GetCurrentProcess().ProcessName;
                    Process[] Procs = Process.GetProcessesByName(strProcessName);
                    foreach (Process item in Procs)
                    {
                        item.Kill();
                    }
                }

                public byte[] ProvideBytes(ushort ust)
                {
                    // Encoding.Unicode
                    return BitConverter.GetBytes(ust);
                }
                public byte[] ProvideBytes(String str)
                {

                    return TextEncode.GetBytes(str);
                }
                public byte[] ProvideBytes(short srt)
                {
                    return BitConverter.GetBytes(srt);
                }
                public byte[] ProvideBytes(uint un)
                {
                    return BitConverter.GetBytes(un);
                }
                public byte[] ProvideBytes(int n)
                {
                    return BitConverter.GetBytes(n);
                }
                //Need Convert byte array to a unsigned Num ?
                public object ProvideNum(byte[] bytArr, int nStartIndex, int nBytesConsist, bool isU)
                {

                    switch (nBytesConsist)
                    {
                        case 2:
                            if (isU)
                                return BitConverter.ToUInt16(bytArr, nStartIndex);
                            else
                                return BitConverter.ToInt16(bytArr, nStartIndex);
                        case 4:
                            if (isU)
                                return BitConverter.ToUInt32(bytArr, nStartIndex);
                            else
                                return BitConverter.ToInt32(bytArr, nStartIndex);
                    }
                    return null;
                }
                public virtual String ProvideString(byte[] bytArr, int nIndex, int nCount)
                {

                    return TextEncode.GetString(bytArr, nIndex, nCount);
                }

                public byte[] ObjectToBytes(object obj)
                {
                    try
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            IFormatter formatter = new BinaryFormatter();
                            formatter.Serialize(ms, obj);
                            return ms.GetBuffer();
                        }
                    }
                    catch (System.Exception ex)
                    {
                        return null;
                    }

                }
                public object BytesToObject(byte[] Bytes, int index, int count)
                {
                    using (MemoryStream ms = new MemoryStream(Bytes, index, count))
                    {
                        IFormatter formatter = new BinaryFormatter();
                        return formatter.Deserialize(ms);
                    }
                }
            }
        }
       
    }
}

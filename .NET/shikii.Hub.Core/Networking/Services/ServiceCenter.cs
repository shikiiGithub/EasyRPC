using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using System.Collections.Concurrent;
using System.IO;
using shikii.Hub.Common;
using System.Diagnostics;

namespace shikii.Hub.Networking
{
    public class ServiceCenter : TCPServer
    {
        public  Dictionary<String, String> RegisteredServices { get; set; }

        ConcurrentDictionary<long, NetworkFileInfo> NetworkFileDic { get; set; }
       
       Dictionary<String, HashSet<String>> RegisteredSpyingServices { get; set; }    

        public ServiceCenter()
        {
            RegisteredServices = new Dictionary<String, String>();
            NetworkFileDic = new ConcurrentDictionary<long, NetworkFileInfo>();
            RegisteredSpyingServices = new Dictionary<string, HashSet<String>>();
       
            this.Route += ServiceCenterRoute;
            this.ClientConnected +=( clientid)=>{
                string strClientId = this.GetClientIP(clientid);
                
                Console.WriteLine(  strClientId  + " is Connected !");
          
            };
            this.ClientDisconnected +=  (clientId)=>
            {
                    //将要断开连接的服务名
                   String serviceName = InternalGetServiceName(clientId);
                if (String.IsNullOrEmpty(serviceName))
                    return;
                    this.NotifyServiceChanged(serviceName, false);
                    RegisteredServices.Remove(serviceName);
                    //如果订阅者服务断开了，则清除其已注册监视的服务列表
                    if (this.RegisteredServices.Count > 0 && RegisteredSpyingServices.Keys.Contains(serviceName))
                        this.RegisteredSpyingServices.Remove(serviceName);
                    Console.WriteLine(serviceName + " is offlined !");
                
             
            };
        }

        /// <summary>
        /// 如果有服务订阅了其它服务状态变更事件 
        /// </summary>
        /// <param name="changedServiceName">服务状态变的服务名</param>
        /// <param name="status">服务状态 true--connect false--disconnect</param>
        void NotifyServiceChanged(String changedServiceName,bool status)
        {
            //如果有服务订阅了其它服务状态变更事件 
            if (this.RegisteredSpyingServices.Count > 0)
            {
                List<HashSet<String>> values = this.RegisteredSpyingServices.Values.ToList();

                for (int i = 0; i < values.Count; i++)
                {
                    HashSet<String> spyingService = values[i];
                    if (spyingService == null)
                        continue;
                    if (spyingService.Contains(changedServiceName))
                    {
                        //取出订阅者服务名
                        List<String> hosts = RegisteredSpyingServices.Keys.ToList();
                        String hostServiceName = hosts[i];
                        //检查订阅者服务是否在线
                        if (this.RegisteredServices.Keys.Contains(hostServiceName))
                        {
                            String _clientId = this.RegisteredServices[hostServiceName];
                            StringBuilder sb = new StringBuilder();
                            sb.AppendFormat("\"{0}\":{1}", changedServiceName, status.ToString().ToLower());
                            sb.Insert(0, '{');
                            sb.Append("}");
                            //发送已经断开连接的状态消息给相应的服务
                            this.Send(_clientId, Signals.SpyingServiceChanged, this.TextEncode.GetBytes(sb.ToString()));
                            sb.Clear(); 
                        }
                    }
                    else
                        continue;

                }
            }
        }
        void ServiceCenterRoute(String clientId,byte msg,byte []buf)
        {
            Socket sct = this.GetClientSocket(clientId);
            switch (msg)
            {
                case Signals.REGISTER_SERVICE: RegisterService(clientId, buf); break;
                case Signals.BYTES_CTC:PassMessageFromCTCBegin(clientId,buf); break;
                case Signals.BYTES_CTC_NoLoop:PassMessageFromCTCEnd(clientId, buf); break;
                case Signals.GET_REGISTERED_SERVICES: GetRegisteredServices(clientId,buf); break;
                case Signals.UploadFileBegin:RecieveFileBegin(buf);break;
                case Signals.UploadingFile: RecievingFile(buf); break;
                case Signals.UploadFileEnd: RecievingFileEnd(buf); break;
                case Signals.DownloadFileRequest:this.SendingFile(clientId, buf); break;
                case Signals.RegisterSpyingService: this.RegisterSpyingServices( clientId, buf); break; //注意你要监视的服务
            }
        }
        private void RecievingFileEnd(byte[] buf)
        {
            byte[] timeStampBytes = new byte[8];
            int z = 0;
            for (int i = buf.Length - 8; i < buf.Length; i++)
            {
                timeStampBytes[z++] = buf[i];
            }
            long timestamp = BitConverter.ToInt64(timeStampBytes);
            
            NetworkFileInfo fi;
            this.NetworkFileDic.TryGetValue(timestamp, out fi);
            fi.ThisFileStream.Flush();
            fi.ThisFileStream.Close();
            fi.ThisFileStream.Dispose();
            NetworkFileInfo outfi;
            this.NetworkFileDic.TryRemove(timestamp, out outfi);
           
        }

        void RecievingFile(byte[] buf)
        {
            byte[] timeStampBytes = new byte[8];
            int len = (int) TCPBase.FetchDataLenByts(buf) ;

            int z = 0;
            for (int i = len - 8; i < len; i++)
            {
                timeStampBytes[z++] = buf[i];
            }
            long timestamp = BitConverter.ToInt64(timeStampBytes);
            NetworkFileInfo fi;
            this.NetworkFileDic.TryGetValue(timestamp, out fi);
            fi.ThisFileStream.Write(buf, TCPBase.MARKPOSITION,len - 8 - TCPBase.MARKPOSITION);
        
            fi.CurrentRecievs ++ ;
        }

        void RecieveFileBegin(byte[] buf)
        {
            String json = TextEncode.GetString(buf, TCPBase.MARKPOSITION, buf.Length - TCPBase.MARKPOSITION );
            NetworkFileBeginInfo info = LitJson.JsonMapper.ToObject<NetworkFileBeginInfo>(json);
            NetworkFileInfo fi = new NetworkFileInfo();
            fi.FileName = info.FileName;
            fi.TotalRecieveTimes = info.TotalTimes;
            FileStream fs = new FileStream(info.FileName, FileMode.OpenOrCreate);
            fi.ThisFileStream = fs;
            fi.CurrentRecievs = 0 ;
            this.NetworkFileDic.TryAdd(info.TimeStamp,fi);

        }

        void SendingFile(String clientId, byte[] buf)
        {
            byte[] timeStampBytes = null;
            int z = 0; 
            String json = TextEncode.GetString(buf, TCPBase.MARKPOSITION, buf.Length - TCPBase.MARKPOSITION );
            NetworkFileBeginInfo info = LitJson.JsonMapper.ToObject<NetworkFileBeginInfo>(json);
            timeStampBytes = BitConverter.GetBytes(info.TimeStamp);
            Socket sct = this.GetClientSocket(clientId);
            byte[] data = null;
            using (FileStream fs = new FileStream(info.FileName, FileMode.OpenOrCreate))
            {
                int nTimes = (int)(fs.Length / info.BufferSize);
                nTimes = (fs.Length % info.BufferSize == 0 ? nTimes : nTimes + 1);
                uint len = (uint)fs.Length;
                int nCount = 0;
                info.TotalTimes = nTimes;
                byte[] contentBytes = this.TextEncode.GetBytes(LitJson.JsonMapper.ToJson(info));
                this.Send(clientId, Signals.DownloadFileBegin, contentBytes);
               
                data = new byte[info.BufferSize + TCPBase.MARKPOSITION + 8];
                while (z < nTimes)
                {
                  
                    if (len - nCount >= info.BufferSize)
                    {
                        fs.Read(data, TCPBase.MARKPOSITION, info.BufferSize);
                        StoreDataLenByts((uint)data.Length, data);
                        timeStampBytes.CopyTo(data, TCPBase.MARKPOSITION + info.BufferSize);
                        StoreMSGMark(data, Signals.DownloadingFile);
                        sct.Send(data);
                    }
                    else
                    {
                        int leftover = (int)len - nCount;
                        byte[] leftOverBytes = new byte[leftover + TCPBase.MARKPOSITION + 8];
                        fs.Read(leftOverBytes, TCPBase.MARKPOSITION, leftover);
                        StoreDataLenByts((uint)(leftover + 8 + TCPBase.MARKPOSITION), leftOverBytes);
                        timeStampBytes.CopyTo(leftOverBytes, leftover + TCPBase.MARKPOSITION);
                        StoreMSGMark(leftOverBytes, Signals.DownloadingFile);
                        sct.Send(leftOverBytes);
                    }

                    z++;
                    nCount += info.BufferSize;
                }
            }
            this.Send(clientId,Signals.DownloadFileEnd,timeStampBytes);

        }

        void GetRegisteredServices(String clientId, byte[] buf)
        {
            List<String> lst = RegisteredServices.Keys.ToList();
            String json = LitJson.JsonMapper.ToJson(lst);
            byte[] content = this.TextEncode.GetBytes(json);
            byte[] byts = buf.Skip(TCPBase.MARKPOSITION).ToArray();
            byte[] messageContent = new byte[TCPBase.MARKPOSITION + content.Length + 8];
            byts.CopyTo(messageContent, TCPBase.MARKPOSITION + content.Length);
            TCPBase.StoreMSGMark(messageContent, Signals.GET_REGISTERED_SERVICES);
            TCPBase.StoreDataLenByts((uint)messageContent.Length, messageContent);
            content.CopyTo(messageContent, TCPBase.MARKPOSITION);
            Socket sct = this.GetClientSocket(clientId);
            sct.Send(messageContent);
        }

        private void PassMessageFromCTCEnd(string clientId, byte[] buf)
        {
            uint nLen = TCPBase.FetchDataLenByts(buf);
            int targetServiceNameStrLen = buf[TCPBase.MARKPOSITION];
            String targetServiceName = this.ProvideString(buf, TCPBase.MARKPOSITION + 1, targetServiceNameStrLen);
            byte[] data = new byte[nLen - targetServiceNameStrLen-1];
            TCPBase.StoreDataLenByts((uint)data.Length, data);
            TCPBase.StoreMSGMark(data, Signals.BYTES_CTC_NoLoop);
            byte[] messageBuf = buf.Skip(TCPBase.MARKPOSITION + targetServiceNameStrLen + 1).ToArray();
            messageBuf.CopyTo(data, TCPBase.MARKPOSITION );
            if (RegisteredServices.Keys.Contains(targetServiceName))
            {
                String targetClientId = RegisteredServices[targetServiceName];
                Socket sct = this.GetClientSocket(targetClientId);
                sct.Send(data);
            }
        }
        void RegisterService(String clientId,byte[]buf)
        {
            uint nLen =TCPBase.FetchDataLenByts(buf);
            String serviceName = this.ProvideString(buf, TCPBase.MARKPOSITION, (int)nLen-TCPBase.MARKPOSITION);
            if(!RegisteredServices.ContainsKey(serviceName))
            {
               RegisteredServices.Add(serviceName, clientId);
                this.NotifyServiceChanged(serviceName, true);
            }
            else
            {
                Console.WriteLine("服务已注册");
            }
           

        }

        String InternalGetServiceName(String clientId)
        {

            if (RegisteredServices.Values.Contains(clientId))
            {
                int nindex = this.RegisteredServices.Values.ToList().IndexOf(clientId);

                String serviceName = RegisteredServices.Keys.ToList()[nindex];
                return serviceName;
            }
            else
                return null;
           
        }
        String GetServiceName(String clientId)
        {
            if (RegisteredServices.Values.Contains(clientId))
            {
                int nindex = this.RegisteredServices.Values.ToList().IndexOf(clientId);

                String serviceName = RegisteredServices.Keys.ToList()[nindex];
                return serviceName;
            }
            else
                throw new Exception("error: please register your service.");
        }
        void PassMessageFromCTCBegin(String clientId, byte[] buf)
        {
            String SourceServiceName = GetServiceName(clientId);
            byte[] SourceServiceNameBytes = this.TextEncode.GetBytes(SourceServiceName);
            uint nLen = TCPBase.FetchDataLenByts(buf);
            int targetServiceNameStrLen = buf[TCPBase.MARKPOSITION];
            String targetServiceName = this.ProvideString(buf, TCPBase.MARKPOSITION + 1, targetServiceNameStrLen);
            byte[] data = new byte[nLen - targetServiceNameStrLen + SourceServiceNameBytes.Length];
            TCPBase.StoreDataLenByts((uint)data.Length, data);
            TCPBase.StoreMSGMark(data, Signals.BYTES_CTC);
            data[TCPBase.MARKPOSITION] = (byte)SourceServiceNameBytes.Length;
            SourceServiceNameBytes.CopyTo(data, TCPBase.MARKPOSITION + 1);
            byte[] messageBuf = buf.Skip(TCPBase.MARKPOSITION + targetServiceNameStrLen + 1).ToArray();
            messageBuf.CopyTo(data, TCPBase.MARKPOSITION + 1 + SourceServiceNameBytes.Count());
            if (RegisteredServices.Keys.Contains(targetServiceName))
            {
                String targetClientId = RegisteredServices[targetServiceName];
                Socket sct = this.GetClientSocket(targetClientId);
                sct.Send(data);
            }
        }


        void RegisterSpyingServices(String clientId,byte [] buf)
        {
            try
            {
                String SourceServiceName = GetServiceName(clientId);
                uint nLen = TCPBase.FetchDataLenByts(buf);
                String rawserviceNames = this.ProvideString(buf, TCPBase.MARKPOSITION, (int)nLen - TCPBase.MARKPOSITION);
                string[] arr = rawserviceNames.Split(';',StringSplitOptions.RemoveEmptyEntries);
                if (arr != null && arr.Length >= 0)
                {
                    checkIsContainsSourceServiceName:;
                    bool isContainsSourceServiceName = this.RegisteredSpyingServices.Keys.Contains(SourceServiceName);
                    if (isContainsSourceServiceName)
                    {
                        HashSet<String> spyingServices = this.RegisteredSpyingServices[SourceServiceName];
                        StringBuilder sb = new StringBuilder();
                        for (int i = 0; i < arr.Length; i++)
                        {
                            spyingServices.Add(arr[i].Trim());
                            if (this.RegisteredServices.ContainsKey(arr[i].Trim()))
                                sb.AppendFormat("\"{0}\":{1},", arr[i].Trim(), true.ToString().ToLower());
                            else
                                sb.AppendFormat("\"{0}\":{1},", arr[i].Trim(), false.ToString().ToLower());
                        }
                        sb.Insert(0, '{');
                        sb.Remove(sb.Length-1, 1);
                        sb.Append('}');
                        //发送已经断开连接的状态消息给相应的服务
                        this.Send(clientId, Signals.SpyingServiceChanged, this.TextEncode.GetBytes(sb.ToString()));
                        sb.Clear();
                    }
                    else
                    {
                        this.RegisteredSpyingServices.Add(SourceServiceName, new HashSet<String>());
                        goto checkIsContainsSourceServiceName;

                    }
                }
                  
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message +  " " + ex.StackTrace);
            }
        }

        String KillMe()
        {
            try
            {
                Process.GetCurrentProcess().Kill();
                return "ok";
            }
            catch (Exception ex)
            {

                return ex.Message + " " + ex.StackTrace;
            }
        }
    }
}

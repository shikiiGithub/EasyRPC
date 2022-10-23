using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using shikii.Hub.DI;
using System.Collections.Concurrent;
using shikii.Hub.Interfaces;
using shikii.Hub.Core;

namespace shikii.Hub.Networking
{
    public class ServiceHost : TCPClient
    {
        public List<ServiceAssemblyInfo> ServiceAssemblyInfos { get; set; }
        public String ServiceName { get; set; }
        public delegate void OnSpyingServiceChangedCallback(LitJson.JsonData dic);

        public event OnSpyingServiceChangedCallback OnSpyingServiceChanged = null;
        public 
        List<byte[]> MessageBuffers = new List<byte[]>();
        ConcurrentDictionary<long, NetworkFileInfo> NetworkFileDic { get; set; }
        Queue<byte[]> CallService_SendingQueue = new Queue<byte[]>();
        Queue<RequestResultInfo> Request_SendingQueue = new Queue<RequestResultInfo>();
        public List<DaemonThreadInfo> DaemonThreads  ;
        int daemonThreadNum = 3;
        /// <summary>
        /// 默认为3
        /// </summary>
        public int DaemonThreadNum { get { return daemonThreadNum; } set { daemonThreadNum = value; } }
        public ServiceHost()
        {
            ServiceAssemblyInfos = new List<ServiceAssemblyInfo>();
            this.Route += ServiceHostRoute;
            this.OnDisconnect += () =>
            {
                new Thread(() =>
                {
                    Console.WriteLine(this.ServiceName + "处于未连接状态,正在重新连接！");
                ExceptionReconnect:;
                    bool bIsConnected = false;
                    while (!bIsConnected)
                    {
                        bIsConnected = this.Reconnect();
                        Thread.Sleep(500);
                    }
                    bool registered = this.RegisterService(this.ServiceName);
                    if (!registered)
                        goto ExceptionReconnect;
                    Console.WriteLine(this.ServiceName + " 已连接状态！");

                }).Start();
                
            };
            NetworkFileDic = new ConcurrentDictionary<long, NetworkFileInfo>();
            new Thread(() => {
                while (true)
                {
                    if(CallService_SendingQueue.Count > 0)
                    {
                        byte[] buf = CallService_SendingQueue.Dequeue();
                        try
                        {
                            this.Client.Send(buf);
                        }
                        catch (Exception ex)
                        {
                        }
                    
                    }
                    Thread.Sleep(10);
                }
            }).Start();
              
            DaemonThreads = new List<DaemonThreadInfo>();  
            
        
        }

        byte[] InternalHandleCTCMessageBegin(byte[] data)
        {
            CTCMessage messageEntity = null;
            try
            {
                String str = this.TextEncode.GetString(data);
                if (String.IsNullOrEmpty(str))
                    str = "";
                messageEntity = LitJson.JsonMapper.ToObject<shikii.Hub.Networking.CTCMessage>(str);
                if (messageEntity == null)
                {

                    messageEntity = new CTCMessage();
                    messageEntity.ErrorMsg = "所传数据格式不正确";
                }
                Action<CTCMessage, Object> InvokeMethod = (entity, host) =>
                {
                    Type type = host.GetType();
                    MethodInfo mif = type.GetMethod(entity.MethodName, BindingFlags.NonPublic | BindingFlags.Instance);
                    if(mif == null)
                    {
                        throw new Exception(String.Format("对于 {0}.{1} 方法名必须相同并且不能以 public 修饰符修饰。", type.FullName, entity.MethodName)) ;
                        return;
                    }
                    Object obj = mif.Invoke(host, entity.Params);
                    entity.ReturnedData = obj;
                };
                DiManager di = (DiManager)shikii.Hub.Common.ExtensionFuncs.ThisDiManager;
                if (di == null)
                    throw new Exception("Di 容器为空");

                Action<CTCMessage, Type[]> LoopFindTargetClassThenInvokeTargetMethod = (entity, types) =>
                {
                    types.ToList().ForEach(t =>
                    {
                        if (t.Name == entity.ClassName)
                        {
                            Type IBootableAssemblyType = ((System.Reflection.TypeInfo)t).ImplementedInterfaces.ToList().Find(x => x.Name == "IBootableAssembly");
                            if (IBootableAssemblyType == null && di.GetService(t) != null )
                            {
                                Object host = di.GetService(t, entity.AliasDiName);
                                InvokeMethod(entity, host);
                            }
                            else
                            {
                                if(IBootableAssemblyType != null)
                                {
                                    Object host = di.GetService(IBootableAssemblyType);
                                    InvokeMethod(entity, host);
                                }
                                else
                                {
                                    di.RegisterType(t, entity.LifeCyleTimeMode, entity.AliasDiName);
                                    Object host = di.GetService(t, entity.AliasDiName);
                                    di.InjectTo(host);
                                    InvokeMethod(entity, host);
                                }
                           
                            }
                        }
                    });
                };

                //如果调用的是主程序未引用的外部程序集
                if (!String.IsNullOrEmpty(messageEntity.AssemblyPath))
                {

                   

                    Assembly asm = di.GetService<Assembly>(Path.GetFileNameWithoutExtension(messageEntity.AssemblyPath));
                    if (asm == null)
                    {
                        asm = Assembly.LoadFrom(messageEntity.AssemblyPath);
                        di.RegisterInstance(asm, asm.GetName().Name);
                    }
                    String location = asm.FullName;
                    String shortTargetAsmName = Path.GetFileNameWithoutExtension(messageEntity.AssemblyName).ToLower();
                    String shortAsmName = Path.GetFileNameWithoutExtension(asm.GetName().Name).ToLower();
                    if (shortAsmName == shortTargetAsmName)
                    {
                        Assembly _asm = Assembly.LoadFrom(location);
                        if (!String.IsNullOrEmpty(messageEntity.ClassName))
                        {
                            Type[] types = _asm.GetTypes();
                            LoopFindTargetClassThenInvokeTargetMethod(messageEntity, types);
                        }
                    }
                }
                else
                {
                    //如果调用的是模块程序集
                    if (!String.IsNullOrEmpty(messageEntity.AssemblyName))
                    {
                       ServiceAssemblyInfo info  = ServiceAssemblyInfos.Find(x=>x.AssemblyName == messageEntity.AssemblyName);
                        if (info == null)
                        {
                            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
                            foreach (Assembly assembly in asms)
                            {
                                if(assembly.GetName().Name == messageEntity.AssemblyName)
                                {
                                    info = new ServiceAssemblyInfo();
                                    info.AssemblyName = assembly.GetName().Name;
                                    info.GatherAssemblyInfo(assembly);
                                    break;
                                }
                            }
                            this.ServiceAssemblyInfos.Add(info);
                        }
                        String specialClassName = String.Format("{0}/{1}", messageEntity.AssemblyName, messageEntity.ClassName);
                        List<ServiceMethodInfo> serviceMethodInfos = info.MethodInfoSet[specialClassName];
                        ServiceMethodInfo _serviceMtdInfo = serviceMethodInfos.Find(x => x.Name == messageEntity.MethodName);
                        Type t = _serviceMtdInfo.ClassType;
                        bool hasRegistered = false;
                        if (t.Name != "App")
                            hasRegistered = di.HasRegistered(t);
                        else
                        {
                            hasRegistered = true;
                        }
                        if(hasRegistered)
                        {
                            Object host = di.GetService(t, messageEntity.AliasDiName);
                            Object obj = _serviceMtdInfo.ThisMethod.Invoke(host, messageEntity.Params);
                            messageEntity.ReturnedData = obj;
                        }
                        else
                        {
                            di.RegisterType(t, messageEntity.LifeCyleTimeMode, messageEntity.AliasDiName);
                            Object host = di.GetService(t, messageEntity.AliasDiName);
                            di.InjectTo(host);
                            Object obj = _serviceMtdInfo.ThisMethod.Invoke(host, messageEntity.Params);
                            messageEntity.ReturnedData = obj;
                        }
                    }
                    else
                    {
                        //如果调用的是本类中的方法
                        if (String.IsNullOrEmpty(messageEntity.ClassName))
                        {
                            InvokeMethod(messageEntity, this);
                        }
                        else //如果调用的是本程序集中其它类中的方法
                        {
                            Type[] types = this.GetType().Assembly.GetTypes();
                            LoopFindTargetClassThenInvokeTargetMethod(messageEntity, types);
                        }
                    }

                }


            }
            catch (Exception ex)
            {

                messageEntity.ErrorMsg = ex.Message + " " + ex.StackTrace;
            }
            String resultString = LitJson.JsonMapper.ToJson(messageEntity);
            return this.TextEncode.GetBytes(resultString);

        }

        private void ServiceHostRoute(string clientId, byte msgKind, byte[] data)
        {
            if (DaemonThreads.Count == 0)
            {
              
                for (int i = 0; i < DaemonThreadNum; i++)
                {
                    AddNewDaemonThread(i);
                }
            }
            switch (msgKind)
            {
                case Signals.BYTES_CTC: InternalHandleCTCMessage(data); break;
                case Signals.BYTES_CTC_NoLoop: InternalHandleCTCMessageNoLoop(data); break;
                case Signals.SpyingServiceChanged:SpyingServiceChanged(data); break;  
                case Signals.DownloadFileBegin: DownloadFileBegin(data); break;
                case Signals.DownloadFileEnd: DownloadFileEnd(data); break;
                case Signals.DownloadingFile: DownloadingFile(data); break;
            }
        }

        void AddNewDaemonThread(int index)
        {
            Thread thd = new Thread(() =>
            {
                Thread _thd = Thread.CurrentThread;
                DaemonThreadInfo _info = DaemonThreads.Find(x => x.ThisThread == _thd);
                while (true)
                {
                    if (_info != null && _info.Tag != null)
                    {
                        if (DaemonThreads.Count() > 0)
                        {

                            if (_info.Tag != null && !_info.IsBusy)
                            {
                                _info.IsBusy = true;
                                RequestResultInfo requestResultInfo = _info.Tag as RequestResultInfo;
                                try
                                {

                                    byte[] byts = requestResultInfo.GetRequestBuffer();
                                    requestResultInfo.RawSendingContent = this.InternalHandleCTCMessageBegin(byts);
                                    byte[] buf = requestResultInfo.GetSendingContent();
                                    this.CallService_SendingQueue.Enqueue(buf);
                                }
                                catch (Exception ex)
                                {

                                    this.CallService_SendingQueue.Enqueue(requestResultInfo.GetErrorBytes(ex));
                                }
                                _info.Tag = null;
                                _info.IsBusy = false;
                            }
                        }
                    }
                    Thread.Sleep(10);
                }
            });
            thd.Name = "Daemon Thread " + index;
            DaemonThreadInfo info = new DaemonThreadInfo();
            info.ThisThread = thd;
            info.IsBusy = false;
            info.Tag = null;
            info.ThisThread.Start();
            DaemonThreads.Add(info);
        }

        void SpyingServiceChanged(byte [] data)
        {
            uint len = TCPBase.FetchDataLenByts(data);
            String str = this.ProvideString(data, TCPBase.MARKPOSITION, (int)len-TCPBase.MARKPOSITION);
            LitJson.JsonData serviceStatusInfo =LitJson.JsonMapper.ToObject(str);
            if(this.OnSpyingServiceChanged != null)
                this.OnSpyingServiceChanged.Invoke(serviceStatusInfo);
        }

        void DownloadingFile(byte[] data)
        {
            byte[] timeStampBytes = new byte[8];
            int len = (int)TCPBase.FetchDataLenByts(data);

            int z = 0;
            for (int i = len - 8; i < len; i++)
            {
                timeStampBytes[z++] = data[i];
            }
            long timestamp = BitConverter.ToInt64(timeStampBytes);
            NetworkFileInfo fi;
            this.NetworkFileDic.TryGetValue(timestamp, out fi);
            fi.ThisFileStream.Write(data, TCPBase.MARKPOSITION, len - 8 - TCPBase.MARKPOSITION);
       
            fi.CurrentRecievs++;
        }

        void DownloadFileEnd(byte[] data)
        {
            byte[] timeStampBytes = new byte[8];
            int z = 0;
            for (int i = data.Length - 8; i < data.Length; i++)
            {
                timeStampBytes[z++] = data[i];
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

        void DownloadFileBegin(byte[] data)
        {
            int len = (int)TCPBase.FetchDataLenByts(data);
            String json = this.TextEncode.GetString(data,TCPBase.MARKPOSITION,len-TCPBase.MARKPOSITION);
            NetworkFileBeginInfo info = LitJson.JsonMapper.ToObject<NetworkFileBeginInfo>(json);
            NetworkFileInfo  networkFileInfo = new NetworkFileInfo();  
            networkFileInfo.FileName = Path.Combine(info.SaveFileDir, Path.GetFileName(info.FileName));
            networkFileInfo.TotalRecieveTimes = info.TotalTimes;
            networkFileInfo.ThisFileStream = new FileStream(networkFileInfo.FileName, FileMode.OpenOrCreate);
            networkFileInfo.CurrentRecievs = 0;
            this.NetworkFileDic.TryAdd(info.TimeStamp, networkFileInfo);
        }

        String GetCurrentAppMemory(String prefix = "当前应用程序占用内存为:")
        {
            double lf = (((double)System.Diagnostics.Process.GetCurrentProcess().WorkingSet64) / 1024) / 1024;
            String tmp = String.Format("{0} {1} MB", prefix, Math.Round(lf, 2));
            return tmp;
        }

        double GetCpuUsageForProcess()
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var endTime = DateTime.UtcNow;
            var endCpuUsage = Process.GetCurrentProcess().TotalProcessorTime;
            var cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            var totalMsPassed = (endTime - startTime).TotalMilliseconds;
            var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
            return cpuUsageTotal * 100;
        }

        String ForceGCCollect()
        {
            GC.Collect();
            return "正在执行垃圾回收";
        }

        void InternalHandleCTCMessageNoLoop(byte[] buf)
        {
            byte[] msgBuf = buf.Skip(TCPBase.MARKPOSITION).ToArray();
            MessageBuffers.Add(msgBuf);
        }
        void InternalHandleCTCMessage(byte[] buf)
        {
            RequestResultInfo requestResultInfo = new RequestResultInfo();
            requestResultInfo.TextEncode =  this.TextEncode;
            requestResultInfo.RawData = buf;
            //Request_SendingQueue.Enqueue(requestResultInfo);
            cc:;
            DaemonThreadInfo info = DaemonThreads.Find(x => x.IsBusy == false);
            if (info != null)
                info.Tag = requestResultInfo; //Request_SendingQueue.Dequeue();
            else
            {
                this.DaemonThreadNum++;
                AddNewDaemonThread(this.DaemonThreadNum);
                goto cc;
            }
        }
        public bool RegisterService(String serviceName)
        {
            this.ServiceName = serviceName; 
            StringBuilder sb = new StringBuilder() ;
            sb.AppendFormat(" {{ \"Name\": \"{0}\", \"ProcId\":{1} }}",serviceName,System.Diagnostics.Process.GetCurrentProcess().Id) ;
            return this.Send(sb.ToString(), Signals.REGISTER_SERVICE);
        }

        /// <summary>
        /// 注册监视服务是否在线事件,请注意服务名与服务名之间用英文分号分隔
        /// </summary>
        /// <param name="serviceNames">服务名与服务名之间用英文分号分隔</param>
        public bool RegisterSpyingService(String serviceNames)
        {
           
            return this.Send(serviceNames, Signals.RegisterSpyingService);
        }
       
        /// <summary>
        /// 调用服务
        /// </summary>
        /// <param name="serviceName">服务名</param>
        /// <param name="message">要调用服务的信息</param>
        /// <param name="timeout_millsecs">超时时间</param>
        /// <param name="waitMillSecs">循环等待反馈结果的时间间隔</param>
        /// <param name="autoTriggerMessageRecieve">注意，如果调用是在TCP/IP线程内，请将其置为true,否则为false</param>
        /// <returns>反馈的结果</returns>
        public CTCMessage CallService(String serviceName, CTCMessage message, int timeout_millsecs = 60000, int waitMillSecs = 10 )
        {
            String jsonStr = LitJson.JsonMapper.ToJson(message);
            byte[] buf = this.TextEncode.GetBytes(jsonStr);
            byte[] serviceNameBytes = TextEncode.GetBytes(serviceName);
            int nLen = TCPBase.MARKPOSITION + serviceNameBytes.Length + buf.Length + 1 + 8;
            byte[] data = new byte[nLen];
            TCPBase.StoreMSGMark(data, Signals.BYTES_CTC);
            TCPBase.StoreDataLenByts((uint)nLen, data);
            data[TCPBase.MARKPOSITION] = (byte)serviceNameBytes.Length;
            buf.CopyTo(data, TCPBase.MARKPOSITION + serviceNameBytes.Length + 1);
            serviceNameBytes.CopyTo(data, TCPBase.MARKPOSITION + 1);
            long taskId = DateTime.Now.Ticks;
            byte[] taskIdBuf = BitConverter.GetBytes(taskId);
            taskIdBuf.CopyTo(data, TCPBase.MARKPOSITION + serviceNameBytes.Length + buf.Length + 1);
            
            this.CallService_SendingQueue.Enqueue(data);
            DateTime timeout_StartDateTime = DateTime.Now;
        
            while (true)
            {
                TimeSpan ts = DateTime.Now - timeout_StartDateTime;
                if (ts.TotalMilliseconds >= timeout_millsecs)
                    break;
                 
                if (MessageBuffers.Count > 0)
                {
                    byte[] targetMessageBuf = MessageBuffers.Find(x =>
                    {
                    byte[] __buf = x;
                    int nlen = __buf.Length;
                    byte[] __timeTicks = new byte[8];
                    int z = 0;
                    for (int i = 8; i > 0; i--)
                    {
                        __timeTicks[z++] = __buf[nlen - i];
                    }

                    long _taskId = BitConverter.ToInt64(__timeTicks);
                    if (_taskId == taskId)
                        return true;
                    else
                        return false;

                });

                    if (targetMessageBuf != null)
                    {
                        MessageBuffers.Remove(targetMessageBuf);
                        byte[] resultBuffer = targetMessageBuf.SkipLast(8).ToArray();
                        String resultStr = this.TextEncode.GetString(resultBuffer);
                        CTCMessage _msg = LitJson.JsonMapper.ToObject<CTCMessage>(resultStr);
                        return _msg;
                    }
                    else
                        continue;
                }

                Thread.Sleep(waitMillSecs);
            }
            CTCMessage msg = new CTCMessage();
            msg.ErrorMsg = "Error: timeout !";

            return msg;

        }

        public List<String> GetAllServices(int waitMillSecs = 10,int timeout_millsecs = 60000)
        {
           
            int nLen = TCPBase.MARKPOSITION + 8;
            byte[] data = new byte[nLen];
            TCPBase.StoreMSGMark(data, Signals.GET_REGISTERED_SERVICES);
            TCPBase.StoreDataLenByts((uint)nLen, data);
            long taskId = DateTime.Now.Ticks;
            byte[] taskIdBuf = BitConverter.GetBytes(taskId);
            taskIdBuf.CopyTo(data, TCPBase.MARKPOSITION);
            int nSend = this.Client.Send(data);
            DateTime timeout_StartDateTime = DateTime.Now;
           
            while (true)
            {
                TimeSpan ts = DateTime.Now - timeout_StartDateTime;
                if (ts.TotalMilliseconds >= timeout_millsecs)
                    break;
                if (MessageBuffers.Count > 0)
                {
                    byte[] targetMessageBuf = MessageBuffers.Find(x =>
                    {
                        byte[] __buf = x;
                        int nlen = __buf.Length;
                        byte[] __timeTicks = new byte[8];
                        int z = 0;
                        for (int i = 8; i > 0; i--)
                        {
                            __timeTicks[z++] = __buf[nlen - i];
                        }

                        long _taskId = BitConverter.ToInt64(__timeTicks);
                        if (_taskId == taskId)
                            return true;
                        else
                            return false;

                    });

                    if (targetMessageBuf != null)
                    {
                        MessageBuffers.Remove(targetMessageBuf);
                        byte[] resultBuffer = targetMessageBuf.SkipLast(8).ToArray();
                        
                        String json = this.TextEncode.GetString(resultBuffer);
                        List<String> services = LitJson.JsonMapper.ToObject<List<String>>(json);
                      
                        return services;
                    }
                    else
                        continue;
                }
                //if(waitMillSecs >0)
                //Thread.Sleep(waitMillSecs);
            }

            return new List<string>() { "Error:Timeout !"};
        }

        public void UploadFile(String filePath, String uploadedFileDir, ref float process, int bufSizePerTurn = 0, int delayTime = 50)
        {
            if (bufSizePerTurn == 0)
                throw new Exception("bufSizePerTurn 必须大于0");

            byte[] data = null;
            long taskId = DateTime.Now.Ticks;
            byte[] taskIdBuf = BitConverter.GetBytes(taskId);
            process = 0;
            using (FileStream fileStream = new FileStream(filePath, FileMode.Open))
            {
                int nTimes = (int)(fileStream.Length / bufSizePerTurn);
                nTimes = (fileStream.Length % bufSizePerTurn == 0 ? nTimes : nTimes + 1);
                NetworkFileBeginInfo info = new NetworkFileBeginInfo();
                String fileName = Path.GetFileName(filePath);
                String savedFilePath = Path.Combine(uploadedFileDir, fileName);
                info.FileName = savedFilePath;
                info.TotalTimes = nTimes;
                info.TimeStamp = taskId;
                info.SaveFileDir = uploadedFileDir;
                String json = LitJson.JsonMapper.ToJson(info);
                byte[] fileInfoBytes = this.TextEncode.GetBytes(json);
                this.Send(fileInfoBytes, Signals.UploadFileBegin);
                float z = 0;
                uint len = (uint)fileStream.Length;
                int nCount = 0;
                data = new byte[bufSizePerTurn + TCPBase.MARKPOSITION + 8];
                while (z < nTimes)
                {
                    // Thread.Sleep(delayTime);
                    if (len - nCount >= bufSizePerTurn)
                    {
                        fileStream.Read(data, TCPBase.MARKPOSITION, bufSizePerTurn);
                        StoreDataLenByts((uint)data.Length, data);
                        taskIdBuf.CopyTo(data, TCPBase.MARKPOSITION + bufSizePerTurn);
                        StoreMSGMark(data, Signals.UploadingFile);
                        this.Client.Send(data);
                    }
                    else
                    {
                        int leftover = (int)len - nCount;
                        byte[] leftOverBytes = new byte[leftover + TCPBase.MARKPOSITION + 8];
                        fileStream.Read(leftOverBytes, TCPBase.MARKPOSITION, leftover);
                        StoreDataLenByts((uint)(leftover + 8 + TCPBase.MARKPOSITION), leftOverBytes);
                        taskIdBuf.CopyTo(leftOverBytes, leftover + TCPBase.MARKPOSITION);
                        StoreMSGMark(leftOverBytes, Signals.UploadingFile);
                        this.Client.Send(leftOverBytes);
                    }

                    z++;
                    process = z / nTimes;
                    process = (float)Math.Round(process, 2);
                    nCount += bufSizePerTurn;
                }
            }

            this.Send(taskIdBuf, Signals.UploadFileEnd);
        }
        public long DownloadFile(String filePath, String saveDir,int bufferSize)
        {
            NetworkFileBeginInfo info = new NetworkFileBeginInfo();
            info.FileName = filePath;
            info.BufferSize = bufferSize;
            info.TimeStamp = DateTime.Now.Ticks;
            info.SaveFileDir = saveDir; 
            String json = LitJson.JsonMapper.ToJson(info);
            this.Send(json, Signals.DownloadFileRequest);
            return info.TimeStamp;
        }
        public NetworkFileInfo GetCurrentDownloadingTaskProcess(long timeStamp)
        {
            NetworkFileInfo fi;
            this.NetworkFileDic.TryGetValue(timeStamp, out   fi);
            return fi;
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

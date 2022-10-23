package System.Networking.Services;

import System.Networking.Signals;
import System.Networking.TCPBase;
import System.Networking.TCPClient;
import com.JavaHost.App;
import com.alibaba.fastjson2.JSON;
import com.alibaba.fastjson2.JSONObject;

import java.io.IOException;
import java.io.OutputStream;
import java.lang.reflect.Constructor;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.util.*;
import java.util.concurrent.*;
import java.util.function.Predicate;
import java.lang.management.ManagementFactory;  
import java.lang.management.RuntimeMXBean; 
import System.*;
public class ServiceHost extends TCPClient {
    public String ServiceName;
    public String SpyingServices;
    HashMap<Long, byte[]> handledResultBufferDic = new HashMap<Long, byte[]>();
    HashMap<String, ClassInfo> registeredObject = new HashMap<String, ClassInfo>();
    public int DaemonThreadNum = 3;
    List<DaemonThreadInfo> DaemonThreads = new ArrayList<>();
    public Predicate<JSONObject> OnSpyingServiceChanged = null;

    public ConcurrentLinkedQueue<byte[]> SendingQueue = new ConcurrentLinkedQueue<byte[]>();

    public ServiceHost(String strIP, int nPort) {
        super(strIP, nPort);
        this.Route = (buf) -> {

            try {
                switch (buf[0]) {
                    case Signals.BYTES_CTC:
                        InternalHandleCTCMessage(buf);
                        break;
                    case Signals.BYTES_CTC_NoLoop:
                        InternalHandleCTCMessageNoLoop(buf);
                        break;
                    case Signals.SpyingServiceChanged:
                        SpyingServiceChanged(buf);
                        break;
                }
                return true;
            } catch (Exception e) {
                return false;
            }
        };
        this.OnServerExit = (client) -> {

            App.thisApp.logTowerServiceEnabled = false;
            new Thread(() -> {
                App.thisApp.LoopConnect();
            }).start();
            return true;
        };
        new Thread(() -> {
            while (true) {
                try {
                    if (SendingQueue.size() > 0)
                        InternalSend(SendingQueue.poll());
                    else {

                        Thread.sleep(50);
                    }
                } catch (Exception e) {
                    e.printStackTrace();
                }
            }

        }).start();
    }

    void AddNewDaemonThread(int index) {
        Thread thd = new Thread(() ->
        {
            Thread _thd = Thread.currentThread();
            DaemonThreadInfo _info = null;
            int nsize = DaemonThreads.size();
            for (int i = 0; i < nsize; i++) {
                if (DaemonThreads.get(i).ThisThread == _thd) {
                    _info = DaemonThreads.get(i);
                    break;
                }
            }
            while (true) {
                if (_info != null && _info.Tag != null) {
                    if (DaemonThreads.size() > 0) {

                        if (_info.Tag != null && !_info.IsBusy) {
                            _info.IsBusy = true;
                            RequestResultInfo requestResultInfo = (RequestResultInfo) _info.Tag;
                            try {
                                byte[] byts = requestResultInfo.GetRequestBuffer();
                                requestResultInfo.RawSendingContent = this.InternalHandleCTCMessageBegin(byts);
                                byte[] buf = requestResultInfo.GetSendingContent();
                                SendingQueue.add(buf);
                            } catch (Exception ex) {
                                SendingQueue.add(requestResultInfo.GetErrorBytes(ex));

                            }
                            _info.Tag = null;
                            _info.IsBusy = false;
                        }
                    }
                }
                try {
                    Thread.sleep(10);
                } catch (Exception e) {

                }

            }
        });
        thd.setName("Daemon Thread " + index);
        DaemonThreadInfo info = new DaemonThreadInfo();
        info.ThisThread = thd;
        info.IsBusy = false;
        info.Tag = null;
        info.ThisThread.start();
        DaemonThreads.add(info);
    }


    void SpyingServiceChanged(byte[] data) {
        int len = BitConverter.ToInt(data, 1);
        String json = this.en.GetString(data, TCPBase.MARKPOSITION, len - TCPBase.MARKPOSITION);
        JSONObject obj = JSON.parseObject(json);
        if (this.OnSpyingServiceChanged != null)
            this.OnSpyingServiceChanged.test(obj);
    }

    /// <summary>
    /// 注册监视服务是否在线事件,请注意服务名与服务名之间用英文分号分隔
    /// </summary>
    /// <param name="serviceNames">服务名与服务名之间用英文分号分隔</param>
    public boolean RegisterSpyingService(String... serviceNames) {
        try {

            if (serviceNames.length > 1)
                this.SpyingServices = String.join(";", serviceNames);
            else if (serviceNames.length == 1)
                this.SpyingServices = serviceNames[0];
            else {
                System.out.println("serviceName不能为空");
                return false;
            }

            byte[] bytArr = this.en.GetBytes(this.SpyingServices);
            byte[] buffer = new byte[bytArr.length + TCPBase.MARKPOSITION];
            BitConverter.CopyTo(bytArr, buffer, TCPBase.MARKPOSITION);
            StoreDataLenByts(buffer.length, buffer);
            StoreMSGMark(buffer, Signals.RegisterSpyingService);
            OutputStream os = this.Client.getOutputStream();
            os.write(buffer);
            os.flush();
            // os.close();
            return true;
        } catch (Exception e) {
            return false;
        }
    }

    void InternalHandleCTCMessageNoLoop(byte[] data) {
        byte[] msgBuf = Skip(data, TCPBase.MARKPOSITION);
        long tick = BitConverter.ToLong(msgBuf, msgBuf.length - 8);
        handledResultBufferDic.put(tick, msgBuf);
    }

    public Object PrepareInstance(String className,String methodName) throws Exception {
        Class classzz = Class.forName(className);
        // 获取构造器对象
        Constructor constructor = classzz.getConstructor();
        // 利用构造器对象创建一个对象
        Object host = constructor.newInstance();
        // 传递需要执行的方法
        Method method = null;
        Method[] mifs = classzz.getMethods();
        for (int i = 0; i < mifs.length; i++) {
            String mtdName = mifs[i].getName();
            if (mtdName.equals(methodName)) {
                method = mifs[i];
                method.setAccessible(true);
                break;
            }
        }
        ClassInfo cif = new ClassInfo();
        cif.ThisClass = classzz;
        cif.Host = host;
        cif.MethodDic.put(methodName, method);
        this.registeredObject.put(className, cif);
        return cif.Host ;
    }
    byte[] InternalHandleCTCMessageBegin(byte[] data) {
        CTCMessage messageEntity = null;
        try {
            String str = this.en.GetString(data);
            if (str == null || str.equals(""))
                str = "";
            messageEntity = JSON.parseObject(str, CTCMessage.class);
            if (messageEntity == null) {

                messageEntity = new CTCMessage();
                messageEntity.ErrorMsg = "所传数据格式不正确";
            }
            if (!this.registeredObject.containsKey(messageEntity.ClassName)) {
                PrepareInstance(messageEntity.ClassName,messageEntity.MethodName);
            }
            ClassInfo cif = this.registeredObject.get(messageEntity.ClassName);
            if (!cif.MethodDic.containsKey(messageEntity.MethodName)) {
                Method method = null;
                Method[] mifs = cif.ThisClass.getMethods();
                for (int i = 0; i < mifs.length; i++) {
                    String mtdName = mifs[i].getName();
                    if (mtdName.equals(messageEntity.MethodName)) {
                        method = mifs[i];
                        method.setAccessible(true);
                        break;
                    }
                }
                cif.MethodDic.put(messageEntity.MethodName, method);
            }
            Method thisMethod = cif.MethodDic.get(messageEntity.MethodName);
            // 如果调用的方法有参数 invoke(o,param1,param2,param3,...)
            Object result = thisMethod.invoke(cif.Host, messageEntity.Params);
            messageEntity.ReturnedData = result;
        } catch (Exception ex) {
            if (messageEntity == null)
                messageEntity = new CTCMessage();
            messageEntity.ErrorMsg = ex.getMessage() + " " + ex.getStackTrace();
        }
        String resultString = JSON.toJSONString(messageEntity);
        return this.en.GetBytes(resultString);
    }

    byte[] Skip(byte[] src, int start) {
        int len = src.length - start;
        byte[] bytes = new byte[len];
        for (int i = start; i < src.length; i++) {
            bytes[i - start] = src[i];
        }
        return bytes;
    }

    byte[] SkipLast(byte[] src, int len) {
        byte[] bytes = new byte[src.length - len];

        for (int i = 0; i < bytes.length; i++) {
            bytes[i] = src[i];
        }
        return bytes;
    }

    void InternalHandleCTCMessage(byte[] buf) {
        if (this.DaemonThreads.size() == 0) {
            for (int i = 0; i < DaemonThreadNum; i++) {
                AddNewDaemonThread(i);
            }
        }
        RequestResultInfo rinfo = new RequestResultInfo();
        rinfo.RawData = buf;
        rinfo.TextEncode = this.en;
        int nNum = DaemonThreads.size();
        DaemonThreadInfo info = null;
        for (int i = 0; i < nNum; i++) {
            if (DaemonThreads.get(i).IsBusy == false) {
                info = DaemonThreads.get(i);
                break;
            }
        }
        if (info == null) {
            this.DaemonThreadNum++;
            AddNewDaemonThread(this.DaemonThreadNum);
            info = DaemonThreads.get(this.DaemonThreadNum);
        }
        info.Tag = rinfo;
    }
   int GetPid() {  
        RuntimeMXBean runtime = ManagementFactory.getRuntimeMXBean();  
        String name = runtime.getName(); // format: "pid@hostname"  
        try {  
            return Integer.parseInt(name.substring(0, name.indexOf('@')));  
        } catch (Exception e) {  
            return -1;  
        }  
    }  
    public boolean Register(String seviceName) {
        try {
         int  pid = GetPid() ;
        StringBuilder sb = new StringBuilder("{");  //此时sb为空白字符串
        // public StringBuilder append(任意类型):添加数据，并返回对象本身
          sb.append("\"Name\": \"").append(seviceName).
         .append("\", \"ProcId\":").append(pid).append("}");
       ; 
            byte[] bytArr = this.en.GetBytes(sb.toString());
            byte[] buffer = new byte[bytArr.length + 5];
            BitConverter.CopyTo(bytArr, buffer, TCPBase.MARKPOSITION);
            StoreDataLenByts(buffer.length, buffer);
            StoreMSGMark(buffer, Signals.REGISTER_SERVICE);
            OutputStream os = this.Client.getOutputStream();
            os.write(buffer);
            os.flush();
            //    os.close();
            return true;
        } catch (Exception e) {
            return false;
        }


    }

    void InternalSend(byte[] data) throws IOException {

        OutputStream os = this.Client.getOutputStream();
        os.write(data);
        os.flush();
    }

    public CTCMessage CallService(String serviceName, CTCMessage message, int timeout_millsecs, int waitMillSecs) {
        try {
            String jsonStr = JSON.toJSONString(message);
            byte[] buf = this.en.GetBytes(jsonStr);
            byte[] serviceNameBytes = this.en.GetBytes(serviceName);
            int nLen = TCPBase.MARKPOSITION + serviceNameBytes.length + buf.length + 1 + 8;
            byte[] data = new byte[nLen];
            TCPBase.StoreMSGMark(data, Signals.BYTES_CTC);
            TCPBase.StoreDataLenByts(nLen, data);
            data[TCPBase.MARKPOSITION] = (byte) serviceNameBytes.length;
            BitConverter.CopyTo(buf, data, TCPBase.MARKPOSITION + serviceNameBytes.length + 1);
            BitConverter.CopyTo(serviceNameBytes, data, TCPBase.MARKPOSITION + 1);
            long taskId = DateTime.GetTicks();
            byte[] taskIdBuf = BitConverter.GetBytes(taskId);
            BitConverter.CopyTo(taskIdBuf, data, TCPBase.MARKPOSITION + serviceNameBytes.length + buf.length + 1);
            this.SendingQueue.add(data);
            long timeout_StartDateTime = new Date().getTime();
            while (true) {
                long nowTimeMillsec = new Date().getTime();
                long ts = nowTimeMillsec - timeout_StartDateTime;
                if (ts >= timeout_millsecs)
                    break;

                if (this.handledResultBufferDic.size() > 0) {
                    if (this.handledResultBufferDic.containsKey(taskId)) {
                        byte[] targetMessageBuf = this.handledResultBufferDic.get(taskId);

                        byte[] resultBuffer = SkipLast(targetMessageBuf, 8);
                        String resultStr = this.en.GetString(resultBuffer);
                        CTCMessage _msg = JSON.parseObject(resultStr, CTCMessage.class);
                        handledResultBufferDic.remove(taskId);
                        return _msg;
                    }

                } else
                    continue;
                Thread.sleep(waitMillSecs);
            }
            CTCMessage msg = new CTCMessage();
            msg.ErrorMsg = "error: timeout !";
            return msg;
        } catch (Exception e) {
            CTCMessage msg = new CTCMessage();
            msg.ErrorMsg = e.getMessage() + " " + e.getStackTrace();

            return msg;
        }

    }

    public List<String> GetAllServices(int waitMillSecs, int timeout_millsecs) {
        List<String> list = null;
        try {
            int nLen = TCPBase.MARKPOSITION + 8;
            byte[] data = new byte[nLen];
            TCPBase.StoreMSGMark(data, Signals.GET_REGISTERED_SERVICES);
            TCPBase.StoreDataLenByts(nLen, data);
            long taskId = DateTime.GetTicks();
            byte[] taskIdBuf = BitConverter.GetBytes(taskId);
            BitConverter.CopyTo(taskIdBuf, data, TCPBase.MARKPOSITION);
            this.SendingQueue.add(data);
            long timeout_StartDateTime = new Date().getTime();
            Date nowTime = new Date();
            while (true) {
                long ts = nowTime.getTime() - timeout_StartDateTime;
                if (ts >= timeout_millsecs)
                    break;
                if (this.handledResultBufferDic.size() > 0) {
                    if (this.handledResultBufferDic.containsKey(taskId)) {
                        byte[] targetMessageBuf = this.handledResultBufferDic.get(taskId);
                        byte[] resultBuffer = SkipLast(targetMessageBuf, 8);
                        String resultStr = this.en.GetString(resultBuffer);
                        list = JSON.parseArray(resultStr, String.class);
                        handledResultBufferDic.remove(taskId);
                        return list;
                    }

                } else
                    continue;
                Thread.sleep(waitMillSecs);
            }
        } catch (Exception e) {
            list = new ArrayList<String>();
            list.add(e.getMessage() + " " + e.getStackTrace());
        }
        list.add("Error:Timeout !");
        return list;
    }

}

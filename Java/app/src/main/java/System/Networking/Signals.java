package System.Networking;

public class Signals {
    public static final byte BYTES_CTS = 0;
    public final byte BYTES_STC = 1;
    public static final byte BYTES_CTC = 2;
    public static final byte BYTES_CTC_NoLoop = 3;
    public final byte FILE_BEGIN = 4;
    public final byte FILE_TRANSFER = 5;
    public final byte FILE_END = 6;
    public final byte DOWNLOAD_FILE = 7;
    public static final byte REGISTER_SERVICE = 8;
    public static final byte GET_REGISTERED_SERVICES = 9;
    public final byte UploadFileBegin = 10;
    public final byte UploadingFile = 11;
    public final byte UploadFileEnd = 12;
    public final byte DownloadFileRequest = 13;
    public final byte DownloadFileBegin = 14;
    public final byte DownloadingFile = 15;
    public final byte DownloadFileEnd = 16;
    public final byte NodeJSWebAPI = 17;
    //注册要监视的服务
    //主要是看服务是否有变化 （连接断开）
    public static final byte RegisterSpyingService = 18;
    /// <summary>
    /// 当所监视的服务变化时（连接/断开）
    /// </summary>
    public static final byte SpyingServiceChanged = 19;
    //执行特定的方法
    public final byte CALL_METHOD = 51;

}

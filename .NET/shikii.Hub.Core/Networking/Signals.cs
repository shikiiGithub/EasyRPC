using System;
using System.Collections.Generic;
using System.Text;
namespace shikii
{
    namespace Hub
    {
        namespace Networking
        {
            public static class Signals
            {

                public const byte BYTES_CTS = 0;
                public const byte BYTES_STC = 1;
                public const byte BYTES_CTC = 2;
                public const byte BYTES_CTC_NoLoop = 3;
                public const byte FILE_BEGIN = 4;
                public const byte FILE_TRANSFER = 5;
                public const byte FILE_END = 6;
                public const byte DOWNLOAD_FILE = 7;
                public const byte REGISTER_SERVICE = 8;
                public const byte GET_REGISTERED_SERVICES = 9;
                public const byte UploadFileBegin = 10;
                public const byte UploadingFile = 11;
                public const byte UploadFileEnd = 12;
                public const byte DownloadFileRequest = 13;
                public const byte DownloadFileBegin = 14;
                public const byte DownloadingFile = 15;
                public const byte DownloadFileEnd = 16;
                public const byte NodeJSWebAPI = 17;
             
                //注册要监视的服务
                //主要是看服务是否有变化 （连接断开）
                public const byte RegisterSpyingService = 18;
                /// <summary>
                /// 当所监视的服务变化时（连接/断开）
                /// </summary>
                public const byte SpyingServiceChanged = 19;
                 
                //执行特定的方法
                public const byte CALL_METHOD = 51;

            }
        }
    }
}
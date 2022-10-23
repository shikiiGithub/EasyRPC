using System;
using System.Collections.Generic;
using System.Text;

namespace shikii.Hub.Networking
{
    public class NetworkFileBeginInfo
    {
        public int TotalTimes { get; set; }
        public String FileName { get; set; }

        public int BufferSize { get; set; }

        public long TimeStamp { get; set; }

        public String SaveFileDir { get; set; }

    }
}

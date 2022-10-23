using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace shikii.Hub. Networking
{
    public class NetworkFileInfo
    {
        public String FileName { get; set; }
       
        public Stream ThisFileStream { get; set; }

        public int TotalRecieveTimes {get;set;}
        public int CurrentRecievs {get;set;}

    }
}

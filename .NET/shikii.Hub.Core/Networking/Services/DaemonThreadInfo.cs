using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace shikii.Hub.Networking
{
    public class DaemonThreadInfo
    {
       public bool IsBusy { get; set; }

       public Thread ThisThread { get; set; }

       public object Tag { get; set; }  
    }
}

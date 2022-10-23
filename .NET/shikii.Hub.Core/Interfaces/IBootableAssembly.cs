using shikii.Hub.Helpers;
using shikii.Hub.Networking;
using System;
using System.Collections.Generic;
using System.Text;

namespace shikii.Hub.Interfaces
{
    public interface IBootableAssembly
    {
        void Boot(DI.DiManager di);

        DI.DiManager ThisDi { get; set; }

        ServiceHost ThisHost { get; set; }

        String ServiceName { get; set; }

        String Description { get; set; }    

    }
}

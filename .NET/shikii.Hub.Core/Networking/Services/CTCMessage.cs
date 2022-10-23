using shikii.Hub.DI;
using System;
using System.Collections.Generic;
using System.Text;


namespace shikii.Hub.Networking
{
    public class CTCMessage
    {
        public string AssemblyPath { get; set; }
        public String AssemblyName { get; set; }

        public String ClassName { get; set; }

        public string MethodName { get; set; }
        public Object[] Params { get; set; }

        public Object ReturnedData { get; set; }
        public String AliasDiName { get; set; } = null;
        public shikii.Hub.DI.DIClassAttribute.LifeCycleModes LifeCyleTimeMode { get; set; } = DIClassAttribute.LifeCycleModes.Singleton;
        public String ErrorMsg { get; set; }

        public void Clean()
        {
            AssemblyPath = null;
            AssemblyName = null;

            ClassName = null;

            MethodName = null;
            Params = null;

            ReturnedData = null;
            AliasDiName = null;
            ErrorMsg = null;
        }

    }
}

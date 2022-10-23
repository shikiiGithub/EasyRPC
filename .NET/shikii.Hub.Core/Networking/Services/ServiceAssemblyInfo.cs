using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace shikii.Hub.Networking {
    public class ServiceMethodInfo
    {
        public Type  ClassType { get; set; }    
        public string Name { get; set; }

        public MethodInfo ThisMethod { get; set; }
    }
    public class ServiceAssemblyInfo
    {

        public String AssemblyName { get; set; }    
        public Dictionary<string, List<ServiceMethodInfo>> MethodInfoSet { get; set; }
        public  ServiceAssemblyInfo()
        {
            MethodInfoSet = new Dictionary<string, List<ServiceMethodInfo>>();
        }

        public void GatherAssemblyInfo(Assembly assembly)
        {
            this.AssemblyName = assembly.GetName().Name;
            Type[] typs = assembly.GetTypes( );
           
            for (int i = 0; i < typs.Length; i++)
            {
                Type t = typs[i];
                if (t.Name.StartsWith("<>"))
                    continue;
                if(t.IsClass)
                {
                    String className = String.Format("{0}/{1}",AssemblyName, t.Name);
                    List<ServiceMethodInfo> infoSet = new List<ServiceMethodInfo>();
                    MethodInfo[] mifs  = t.GetMethods( BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.DeclaredOnly);
                    for (int j = 0; j < mifs.Length; j++)
                    {
                        if (mifs[j].Name.StartsWith("get_") || mifs[j].Name.StartsWith("set_"))
                            continue;
                        String mtdName = mifs[j].Name;
                        ServiceMethodInfo info = new ServiceMethodInfo();
                        info.ClassType = t;
                        info.Name = mtdName;    
                        info.ThisMethod = mifs[j];
                        infoSet.Add( info);
                    }
                    MethodInfoSet.Add(className, infoSet);
                }

            }
        }

    }
}

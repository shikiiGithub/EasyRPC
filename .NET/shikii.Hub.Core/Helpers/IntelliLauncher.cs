using shikii.Hub.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
namespace shikii.Hub.Helpers
{
 
    public class IntelliLauncher
    {
        public  const string JavaProgramExecutor = "java";
        public const string DotnetProgramExecutor = "dotnet";
        public const string WinProgramExecutor = "exe";
        public const string NodeJsProgramExecutor = "Node";

        public Process Boot(String programName,params String [] pars)
        {
            String extension = Path.GetExtension(programName).ToLower();
            Func<String,String,String[],Process> bootAction = (executor,name,_pars) =>
            {
                
                if(_pars.Length == 0)
                {
                   return  FileSystemManager.SilentStart(executor,name);
                }
                else
                {
                    String args = String.Format(" {0} {1} ", name, String.Join(' ', _pars));
                   return   FileSystemManager.SilentStart(executor, args);
                }
            };

           

            switch (extension)
            {
                case ".jar":
                    String[] newPars = null;
                    if (pars.ToList().Contains("-jar"))
                    {
                        newPars = new string[pars.Count()+1];
                        newPars[0] = "-jar";
                        for (int i = 0; i < pars.Length; i++)
                        {
                            newPars[i + 1] = pars[i];
                        }
                    }
                    else 
                        newPars = pars;
                    bootAction(JavaProgramExecutor,programName, newPars); 
                    break;
                case ".dll": return bootAction(DotnetProgramExecutor, programName, pars);  
                case ".js": return  bootAction(NodeJsProgramExecutor, programName, pars);
                case ".exe":
                    //programName = Path.ChangeExtension(programName, ".dll");
                    //return bootAction(DotnetProgramExecutor, programName, pars);
                    if (pars.Length > 0)
                    {
                        String args = String.Join(' ', pars);
                        return FileSystemManager.SilentStart(programName, args);
                    }
                    else
                    {
                        return FileSystemManager.SilentStart(programName);
                    }

            }
            return null;
        }
    }
}

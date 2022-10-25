# EasyRPC

I don't want to use something like Apache Thrift or Google gRPC,because they are so heavy , I only want a simple and light  RPC library ,so i've made it for me. 

## Features 

+ **Roles**

  It can be used as a RPC , Registering  Service and Discovery Other Services.

+ **Architecture**

  Base on TCP/IP protocol , one Service Center and many services .

+ **Easy** 

  Don't need to learn some concepts ,you just need recalling your service name , method ,Args that you need to pass they to the method you want to call.

+ **Support 4 Programming Language Client**

  + C++ ( including ServiceCenter and ServiceHost )

    it can compile to Windows Exe or Linux distributed package.

  + C# ( including ServiceCenter and ServiceHost )

  + Java (only ServiceHost )

  + NodeJs ( ServiceHost and  Light Http Web Server)

    It means you can set up your http Web Server to call a  service which write in other languages. 

## Tutorial

### Step 1 : Compile your ServiceCenter

You can compile the dotnet code to get a  ServiceCenter which means a TCP Server part,but I highly recommend you to compile the ServiceCenter implemented in C++.You can find that in  **CPP/ServiceCenter** , that is a cmake project which means you must install cmake and c++ compiler and config they to compile that part. After compiling , you can get an executable app (for windows,you'll get a **ServiceCenter.exe** file and for Linux , you'll get a **ServiceCenter** named file).

### Step 2 : Prepare your Client A 

Here,  I prefer to use dotnet client  which I can use my favorite programing language **C#**  as my Client A . 

+ 1st ,  Go to  **.NET/shikii.Hub.Core**  folder and compile it into a Assembly DLL which our C# client A will references it.

+ 2nd , Create a dotnet Console App which can use VSCode or Visual Studio to write your code.

  ```c#
  using shikii.Hub.DI;
  using System;
  using System.Collections.Generic;
  using System.Diagnostics;
  using System.IO;
  using System.Reflection;
  using System.Linq;
  using shikii.Hub.Interfaces;
  using System.Threading;
  using shikii.Hub.Networking;
  using LitJson;
  using shikii.Hub.Common;
  using System.Runtime.InteropServices;
  using System.Text; 
  namespace shikii.Hub.Loader
  {
  public class Program : IBootableAssembly
      {
          public DiManager ThisDi { get; set; }
          public ServiceHost ThisHost { get; set; }
          public string ServiceName { get; set; }
          public string Description { get; set; }
          public void Boot(DiManager di)
          {
             //todo:write your own code here !
               
          }
          // RPC Method which can be called by the other service 
          // remember the method modifier must be  private 
          String Hello ()
          {
              try
              {
                 return "Hello from Client A writed in C#";
              }
              catch (Exception ex)
              {
                 return "error:" +ex.Message+ " " + ex.StackTrace;
  
              }
          }
          public static void Main(params string[] args)
          {
                   DiManager diManager = null;
                  //init the Di Container
                  if (ThisDiManager == null)
                  {
                       diManager = new DiManager();
                      ThisDiManager = diManager;
                  }
                  else 
                      diManager = ThisDiManager;
                  //scan your needing injecting class 
                  diManager.Prepare(this.GetType().Assembly);
                  //read the Service config  
                  String bootableAssemblyList = File.ReadAllText("bootableAssemblyList.json", System.Text.Encoding.UTF8);
                  JsonData data = LitJson.JsonMapper.ToObject(bootableAssemblyList);
                  JsonData serviceCenterConfig = data["serviceCenter"];
                  String ip = (String)serviceCenterConfig["ip"];
                  int port = (int)serviceCenterConfig["port"];
                  Program targetClassInstance = new Program() ;
                  diManager.RegisterInstance(_targetClassInstance);
                  targetClassInstance.ThisHost = new ServiceHost();
                  targetClassInstance.ThisHost.IP = ip;
                  targetClassInstance.ThisHost.Port = port;
                  targetClassInstance.ThisHost.BufferSize = (int)serviceCenterConfig["bufferSize"];
                  targetClassInstance.ThisDi = ThisDiManager;
                  targetClassInstance.ThisDi.InjectTo(targetClassInstance);
                  //connect to ServiceCenter
                  bool bConnect = false;
                   bConnect = targetClassInstance.ThisHost.Connect();
                  while (!bConnect)
                  {
                      bConnect = targetClassInstance.ThisHost.Reconnect();
                      Thread.Sleep(500);
                  }
                   string serviceName = "CSharp Client A";
                  // register your service to ServiceCenter
                  targetClassInstance.ThisHost.RegisterService(serviceName);
                  targetClassInstance.Boot(ThisDiManager);
                  Console.ReadLine() ;
          }
  
  
      }
  }
  ```
  
   
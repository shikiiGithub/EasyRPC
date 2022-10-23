package com.JavaHost.Helpers;

import org.quartz.Job;
import org.quartz.JobDataMap;
import org.quartz.JobExecutionContext;
import System.Networking.Services.CTCMessage;
import System.Networking.Services.ServiceHost;


public class CommonJob implements Job {

    @Override
    public void execute(JobExecutionContext context)   {
        try{
            JobDataMap dataMap = context.getJobDetail().getJobDataMap();
            ServiceHost host = (ServiceHost) dataMap.get("serviceHost") ;
            TimerInfo timerInfo = (TimerInfo) dataMap.get("timerInfo") ;
            CTCMessage message = new CTCMessage() ;
            message.AssemblyName= timerInfo.ServiceName;
            message.ClassName = timerInfo.InvokingClassName;
            message.MethodName = timerInfo.InvokingMethodName ;
            message.Params = null ;
            host.CallService(timerInfo.ServiceName,message, 30000,10) ;
        }
        catch (Exception e)
        {
            e.printStackTrace();
        }
    }
}

package com.JavaHost.Helpers;

import com.alibaba.fastjson2.JSON;
import org.quartz.*;
import org.quartz.impl.StdSchedulerFactory;
import System.Networking.Services.ServiceHost;

import java.util.*;

public class NQuartz {

   HashMap<String,TimerInfo> TimerInfoDic;
   public static ServiceHost thisHost ;
    Scheduler scheduler;
    StdSchedulerFactory  stdSchedulerFactory;

    public NQuartz()
    {
          TimerInfoDic = new HashMap<>();
          InternalCreateScheduler();
    }

    /**
     *
     * @return json 后的字符串数组
     */
    public String GetTimerIds()
    {
        if(this.TimerInfoDic.size()>0)
        {
            Set<String> keySet = TimerInfoDic.keySet() ;
            return JSON.toJSONString(keySet);
        }
        else
            return "" ;
    }

     void InternalCreateScheduler()
     {
         try {
               stdSchedulerFactory = new StdSchedulerFactory() ;
               scheduler = stdSchedulerFactory.getScheduler();
               scheduler.start();

         } catch (SchedulerException e) {
             e.printStackTrace();

         }

     }

    /** 创建任务定时器
     * @param timerId 定时器名
     * @param serviceName 服务名
     * @param invokingClassName 当定时器触发后要触发调用哪一个RPC方法所在的类
     * @param invokingMethodName 当定时器触发后要触发调用哪一个RPC方法
     * @param cron cron 表达式
     * @return 如果创建定时器成功（不会自动启用）
     */
    public String NewTimer(String timerId,String serviceName,String invokingClassName,String invokingMethodName,String cron )
    {

        try{

            TimerInfo info = new TimerInfo() ;
            JobKey jobKey = JobKey.jobKey(timerId);
            JobDetail jobDetail = JobBuilder.newJob(CommonJob.class).withIdentity(jobKey)
                    // 任务名称和组构成任务key
                    .build();
            jobDetail.getJobDataMap().put("timerInfo",info);
            jobDetail.getJobDataMap().put("serviceHost",thisHost);
            Trigger trigger = TriggerBuilder.newTrigger()
                    .withSchedule(CronScheduleBuilder.cronSchedule(cron))
                    .build();
            scheduler.scheduleJob(jobDetail,trigger);
            info.TimerId = timerId;
            info.ThisStdSchedulerFactory = stdSchedulerFactory ;
            info.ThisScheduler = scheduler ;
            info.ThisJobDetail = jobDetail ;
            info.ServiceName = serviceName ;
            info.InvokingClassName = invokingClassName ;
            info.InvokingMethodName = invokingMethodName ;
            info.ThisTimerJobKey = jobKey;
            TimerInfoDic.put(timerId,info);
            return "" ;
        }
        catch (Exception e)
        {
            return GetExceptionString(e) ;
        }
    }

    /** JavaHost 内部创建任务定时器
     * @param timerId 定时器名
     * @param IJobClass IJob 类
     * @param cron cron 表达式
     * @return 如果创建定时器成功（不会自动启用）
     */
    public boolean InternalNewTimer(String timerId ,Class IJobClass,String cron )
    {

        try{
            JobKey jobKey = JobKey.jobKey(timerId);
            JobDetail jobDetail = JobBuilder.newJob(IJobClass).withIdentity(jobKey)
                    // 任务名称和组构成任务key
                    .build();
            jobDetail.getJobDataMap().put("serviceHost",thisHost);
            Trigger trigger = TriggerBuilder.newTrigger()
                    .withSchedule(CronScheduleBuilder.cronSchedule(cron))
                    .build();
            scheduler.scheduleJob(jobDetail,trigger);
            return true ;
        }
        catch (Exception e)
        {
            return false ;
        }
    }
    String GetExceptionString(Exception e)
    {
        e.printStackTrace();
        String errorMsg = String.format("error:%s %s ",e.getMessage(),e.getStackTrace()) ;
        return errorMsg ;
    }
    TimerInfo GetTimerInfo(String timerId)
    {
        try{
            if(timerId != null && timerId.trim().equals( ""))
            {
                Collection<TimerInfo> timerInfos  = this.TimerInfoDic.values() ;
                int len = timerInfos.size() ;
                TimerInfo  _tif=null ;
                Object [] objs  = timerInfos.toArray() ;
                for (int i = 0; i < len; i++) {
                    TimerInfo tif = (TimerInfo) objs[i] ;
                    if(tif.TimerId.equals(timerId))
                    {
                        _tif = tif  ;
                        break;
                    }

                }

                return _tif ;
            }
            else
            {
                return null;
            }
        }
        catch ( Exception e)
        {
            e.printStackTrace();
            return null ;
        }
    }

    public String Resume(String timerId)
    {
        try{
            TimerInfo tif = this.GetTimerInfo(timerId) ;
            if(tif != null  )
            {

                tif.ThisScheduler.resumeJob(tif.ThisTimerJobKey);

                return "" ;
            }
            else
            {
                return "error:没有根据所给的TimerId找到相应的定时器";
            }
        }
        catch ( Exception e)
        {
            return GetExceptionString(e) ;
        }
    }

    public String Shutdown(String timerId)
    {
        try{
            TimerInfo tif = this.GetTimerInfo(timerId) ;
            if(tif != null  )
            {
                tif.ThisScheduler.shutdown();
                return "" ;
            }
            else
            {
                return "error:没有根据所给的TimerId找到相应的定时器";
            }
        }
        catch ( Exception e)
        {
            return GetExceptionString(e) ;
        }
    }

    public String Pause(String timerId)
    {
        try{
            TimerInfo tif = this.GetTimerInfo(timerId) ;
            if(tif != null  )
            {
                tif.ThisScheduler.pauseJob(tif.ThisTimerJobKey);

                return "" ;
            }
            else
            {
                return "error:没有根据所给的TimerId找到相应的定时器";
            }
        }
        catch ( Exception e)
        {
            return GetExceptionString(e) ;
        }
    }

}

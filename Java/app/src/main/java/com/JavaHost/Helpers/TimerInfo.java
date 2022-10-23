package com.JavaHost.Helpers;

import org.quartz.JobDetail;
import org.quartz.JobKey;
import org.quartz.Scheduler;
import org.quartz.impl.StdSchedulerFactory;

public class TimerInfo {
    public String TimerId ;
    public StdSchedulerFactory ThisStdSchedulerFactory ;
    public Scheduler ThisScheduler ;
    public JobDetail  ThisJobDetail ;
    public String ServiceName ;
    public String InvokingClassName;
    public String InvokingMethodName ;
    public JobKey ThisTimerJobKey=null;
}

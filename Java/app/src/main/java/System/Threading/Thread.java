package System.Threading;

import System.*;

/**
 * Created by Shikii on 2017/11/20.
 */
public class Thread extends Delegate
{
    String MethodName   ;
    Object Host ;
    RawThread thd_Main = null ;
    public Object [] Params = null ;
    public boolean bIsObjectArray = false ;
    public Object returnObj = null ;
    public  Thread(Object obj, String strMethodName)
    {
        thd_Main =new RawThread() ;
        MethodName = strMethodName ;
        this.Host = obj ;
    }
    public Thread() {thd_Main =new RawThread() ;}

    @Override
    protected void finalize() throws Throwable {
        super.finalize();
        Dispose();
    }

    public void Start()
    {
        thd_Main.start();

    }
    public void Suspend()
    {
        thd_Main.suspend();
    }
    public void Resume()
    {
        if(thd_Main.isAlive())
            thd_Main.resume();
    }
    public void Stop()
    {
        if(thd_Main.isAlive())
            thd_Main.stop();
    }
    public  void Dispose()
    {
        if(thd_Main.isAlive()) {
            thd_Main.stop();
         //   thd_Main.destroy();
        }
    }

    public static void Sleep(int millisecond)
    {
        try {
            java.lang.Thread.sleep(millisecond);
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
    }
    class RawThread extends java.lang.Thread {
        @Override
        public void run() {
            // TODO Auto-generated method stub
            returnObj = ExecuteTask(Host,MethodName,Params,bIsObjectArray);
        }

    }
}



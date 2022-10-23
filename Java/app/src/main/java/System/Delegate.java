package System;

import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;

/**
 * Created by Shikii on 2017/11/21.
 */
public class Delegate {

    public Method ParseMethod(Object obj, String strMethodName)
    {

        Method mtd = null  ;

        Method[] mtds = obj.getClass().getMethods() ;

        for (int i = 0; i < mtds.length; i++) {
            if(mtds[i].getName().equals(strMethodName))
            {
                mtd = mtds[i];
                mtd.setAccessible(true);
                break;

            }

        }
        return mtd ;
    }
    public Method GetMethod(String strMethodName)
    {
        Method[] mtds = this.getClass().getDeclaredMethods() ;
        for (int i = 0; i < mtds.length; i++) {
            if(mtds[i].getName().equals(strMethodName))
            {
                mtds[i].setAccessible(true);
                return mtds[i] ;

            }
        }
        return null ;
    }
    public static Method FetchMethod(Class cls, String strMethodName)
    {
        Method[] mtds = cls.getDeclaredMethods() ;
        for (int i = 0; i < mtds.length; i++) {
            if(mtds[i].getName().equals(strMethodName))
            {
                return mtds[i] ;
            }
        }
        return null ;
    }
    public static Method FetchMethod(Object cls, String strMethodName)
    {
        Method[] mtds = cls.getClass().getDeclaredMethods() ;
        for (int i = 0; i < mtds.length; i++) {
            if(mtds[i].getName().equals(strMethodName))
            {

                return mtds[i] ;

            }
        }
        return null ;
    }
    public static Class GetSpecificClass(String strPackageClassName)
    {
        try {
            return Class.forName(strPackageClassName) ;
        } catch (ClassNotFoundException e) {
            e.printStackTrace();
        }
        return null ;
    }
    // bIsObjectArray 是否是被调用的方法的参数是否形如： Type [] types 或者是 Type ... types
    protected Object ExecuteTask(Object host,String strMethodName,Object []  _Params,boolean bIsObjectArray)
    {
        Method _mtd_Thread_Run = null ;

        if(_mtd_Thread_Run == null)
        {
            if(host!= null && strMethodName!= null)
            {
                _mtd_Thread_Run    = ParseMethod(host,strMethodName);
                try {
                    if(_Params !=null) {
                        if(_Params.length>1)
                        {
                            if(bIsObjectArray)
                                return  _mtd_Thread_Run.invoke(host, (Object) _Params);
                            else
                                return _mtd_Thread_Run.invoke(host,  _Params);
                        }
                        else
                            return _mtd_Thread_Run.invoke(host,  _Params);
                    }
                    else
                        return _mtd_Thread_Run.invoke(host) ;
                } catch (IllegalAccessException e) {
                    e.printStackTrace();
                    return null ;
                } catch (InvocationTargetException e) {
                    e.printStackTrace();
                    return null ;
                }
            }
        }
        else
        {
            try {
                if(_Params !=null)
                    return   _mtd_Thread_Run.invoke(host,_Params) ;
                else
                    return  _mtd_Thread_Run.invoke(host) ;
            } catch (IllegalAccessException e) {
                e.printStackTrace();
                return null ;
            } catch (InvocationTargetException e) {
                e.printStackTrace();
                return null ;
            }

        }
        return null ;
    }

}

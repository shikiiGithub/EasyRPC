package System.Networking.Services;

import java.lang.reflect.Method;
import java.util.Hashtable;

public class ClassInfo {
   public Class ThisClass ;
    public Hashtable<String,Method> MethodDic ;
    public Object Host ;

    public ClassInfo()
    {
        MethodDic = new Hashtable<String,Method>() ;

    }
}

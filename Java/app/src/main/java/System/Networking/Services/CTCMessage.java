package System.Networking.Services;

public class CTCMessage
{
    public String AssemblyPath ;
    public String AssemblyName;
    /**
     * 注意是 包名 + 类名
     */
    public String ClassName;
    public String MethodName ;
    public Object[] Params ;
    public Object   ReturnedData  ;
    public String AliasDiName = null;
    public  LifeCycleModes LifeCyleTimeMode  =  LifeCycleModes.Singleton;
    public String ErrorMsg ;

}

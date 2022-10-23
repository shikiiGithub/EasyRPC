package System;

public class Environment {
    public static   String OSVersion = null ;

    public  static  String NewLine()
    {
        String newLineMark = null ;
        if(OSVersion== null)
        {
            String os = java.lang.System.getProperty("os.name");
            if(os.toLowerCase().startsWith("win")){
                newLineMark = "\r\n" ;
            }
            else
                newLineMark="\n" ;

        }

        return newLineMark ;
    }

}

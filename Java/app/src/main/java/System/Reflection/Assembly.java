package System.Reflection;

/**
 * Created by shikii on 2019/4/30.
 */

import java.net.URL;
import java.net.URLClassLoader;

public class Assembly {
    ClassLoader loader = null;

    private Assembly(ClassLoader loader)
    {
        this.loader = loader ;
    }
    public static Assembly LoadFile(String strJarFilePath)
    {

        try {
            // String dirPath = Path.GetDirectoryName(strJarFilePath) ;
            /*动态载入指定类*/
            java.io.File file=new java.io.File(strJarFilePath);
            URL url=  file.toURI().toURL();
            ClassLoader loaderx=new URLClassLoader(new URL[]{url});//创建类载入器
            Assembly asm = new Assembly(loaderx) ;
            return asm ;
        } catch ( Exception e) {
            e.printStackTrace();
            return null ;
        }
    }

    //包括包名
    public   Class  GetType(String fullClassName)
    {

        try {

            Class  cls=loader.loadClass(fullClassName);

            return cls ;
        } catch (ClassNotFoundException e) {
            e.printStackTrace();
            return null ;
        }
    }

}


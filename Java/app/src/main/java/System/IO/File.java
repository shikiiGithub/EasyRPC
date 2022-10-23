package  System.IO;

import  System.Text.Encoding;

import java.io.*;
import java.lang.reflect.InvocationTargetException;
import java.lang.reflect.Method;
import java.util.ArrayList;
import java.util.List;

public class File {
    static Class class_Android_Environment = null ;
    static Method mtd_static_getExternalStorageDirectory=null ;

    public static boolean   Exists(String strFilePath)
    {
        java.io.File it  = new java.io.File(strFilePath) ;

        return  it.exists() ;
    }

    public  static long GetFileLength(String FileName)
    {
        java.io.File it = new java.io.File(FileName) ;
        return  it.length() ;
    }
    public  static void Rename(String FileName, String sFileName)
    {
        java.io.File it = new java.io.File(FileName) ;

        it.renameTo(new java.io.File(sFileName) ) ;
    }
    public  static void Move(String FileName, String sFileName)
    {
        java.io.File it = new java.io.File(FileName)  ;
        it.renameTo(new java.io.File(sFileName) ) ;

    }
    public  static void Delete(String FileName)
    {
        java.io.File it = new java.io.File(FileName) ;
        it.delete() ;
    }
    public static String[]  ReadAllLines(String strFileName, Encoding en)
    {
        try
        {
            List<String> lst_FileData = new ArrayList<String>() ;
            String str = null ;
            // FileReader it =  new FileReader(strFileName) ;
            InputStreamReader it = new InputStreamReader(new FileInputStream(strFileName),en.toString());
            BufferedReader it_Mem_File =
                    new BufferedReader(it) ;
            while ((str=it_Mem_File.readLine())!=null)
                lst_FileData.add(str) ;
            it_Mem_File.close() ;
            it.close() ;
            String [] arr = new String[lst_FileData.size()] ;
            return  lst_FileData.toArray(arr) ;
        }
        catch(Exception ex)
        {
            ex.printStackTrace();
            return null ;
        }

    }
    public static void WriteAllLine(String FileName, String[] FileContent,Encoding en)
    {
        try
        {
            PrintStream WriteMyFile = new PrintStream(FileName,en.toString()) ;
            int nFileLines = FileContent.length ;
            if(nFileLines != 0)
            {
                for(int i = 0 ;i<nFileLines;i++)
                    WriteMyFile.println(FileContent[i]);
            }
            WriteMyFile.close();
        }
        catch(Exception ex)
        {
            ex.printStackTrace();
        }

    }
    public static void WriteAllText(String FileName, String FileContent, Encoding en)  {

        try
        {
            PrintStream WriteMyFile = new PrintStream(FileName,en.toString()) ;
            if(FileContent.length() != 0)
            {
                WriteMyFile.print(FileContent);
            }
            WriteMyFile.close();
        }
        catch(IOException ex)
        {
            ex.printStackTrace();
        }

    }
    public static String ReadAllText(String FileName,Encoding en)
    {

        try
        {
            BufferedReader in= null;

            in = new BufferedReader(new InputStreamReader(new FileInputStream(FileName),en.toString()));


            String Templine="";
            String ResultString ="" ;
            while((Templine=in.readLine())!=null)
            {
                ResultString += Templine ;
            }
            in.close();
            return ResultString ;
        }
        catch(Exception ex)
        {
           return null ;
        }

    }


    //For shikii.app.Android
    //android.os
    public static boolean isExternalSDCardExist()
            throws ClassNotFoundException, NoSuchMethodException, InvocationTargetException,
            IllegalAccessException {
        if(class_Android_Environment==null)
        {
            class_Android_Environment = Class.forName("android.os.Environment") ;
            mtd_static_getExternalStorageDirectory =
                    class_Android_Environment.getMethod("getExternalStorageDirectory") ;
        }
        java.io.File dir = (java.io.File)mtd_static_getExternalStorageDirectory.invoke(null) ;
//        java.io.File it = new File( dir
//                .getParentFile(),"/sdcard1/" );

          String strPath = Path.Combine(dir.getParent(),"/sdcard1/" ) ;
        return File.Exists(strPath) ;
    }

}

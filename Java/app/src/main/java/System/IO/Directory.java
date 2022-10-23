package System.IO;


import System.Delegate;
import System.*;

import java.io.File;
import java.lang.reflect.Method;
import java.nio.channels.FileChannel;
import java.util.ArrayList;
import java.util.List;

public class Directory {
    static String[] str = null;

    public static boolean IsDirectory(String path) {
        File it = new File(path);
        boolean b = it.isDirectory();

        return b;
    }

    public static boolean CreateDirectory(String DirectoryName) {

        File it = new File(DirectoryName);
        boolean b = it.mkdirs();

        if (!b)
            Console.WriteLine("SQLite_DB_Create_This_App_Directory_Failed !");
        return b;


    }

    static String[] EverythingNames(String DirectoryName) {
        File it = new File(DirectoryName);
        str = it.list();
        return str;
    }

    public static Boolean Exsits(String DirectoryName) {
        File it = new File(DirectoryName);
        return it.exists();
    }

    public static void Delete(String DirectoryName) {
        File it = new File(DirectoryName);
        it.delete();

    }

    public static void Delete(String path, boolean recursive) {

        if (!recursive) {
            Delete(path);
            return;
        }
        File file = new File(path);
        if (file.exists()) {
            File[] files = file.listFiles();
            if (files.length == 0) {
                Delete(path);
                return;
            } else {
                for (File file2 : files) {
                    if (file2.isDirectory()) {
                        System.out.println("�ļ���:" + file2.getAbsolutePath());
                        Delete(file2.getAbsolutePath(), true);

                    } else {
                       Delete(file2.getAbsolutePath());

                    }

                }
                Delete(file.getAbsolutePath());
            }
        } else {
            System.out.println("�ļ�������!");
        }

    }

    //�ݹ�ɾ��
    static void deleteDir(File dir) {
        if (dir.isDirectory()) {
            File[] files = dir.listFiles();
            for (int i = 0; i < files.length; i++) {
                deleteDir(files[i]);
            }
        }
        dir.delete();
    }


    public static String[] GetFiles(String DirectoryName) {

        ArrayList MyList = new ArrayList();
        String[] Temp = null;
        File[] AllFileNames = null;
        File it = new File(DirectoryName);
        AllFileNames = it.listFiles();
        int Num = AllFileNames.length;
        for (int i = 0; i < Num; i++) {
            if (AllFileNames[i].isDirectory())
                continue;
            else
                MyList.add(AllFileNames[i].getAbsolutePath());
        }
        Temp = new String[MyList.size()];
        MyList.toArray(Temp);
        return Temp;

    }

    public static String[] GetDirectories(String DirectoryName) {

        ArrayList MyList = new ArrayList();
        String[] Temp = null;
        File[] AllFileNames = null;
        File it = new File(DirectoryName);
        AllFileNames = it.listFiles();
        int Num = AllFileNames.length;
        for (int i = 0; i < Num; i++) {
            if (AllFileNames[i].isDirectory())
                MyList.add(AllFileNames[i].getAbsolutePath());

        }
        Temp = new String[MyList.size()];
        MyList.toArray(Temp);
        return Temp;

    }

    public static String[] GetFiles(String DirectoryName, String endWith) {

        ArrayList MyList = new ArrayList();
        String[] Temp = null;
        File[] AllFileNames = null;
        File it = new File(DirectoryName);
        AllFileNames = it.listFiles();
        int Num = AllFileNames.length;
        for (int i = 0; i < Num; i++) {
            if (AllFileNames[i].isDirectory())
                continue;
            else {
                String file = AllFileNames[i].getAbsolutePath();
                if (file.endsWith(endWith))
                    MyList.add(file);
            }
        }
        Temp = new String[MyList.size()];
        MyList.toArray(Temp);
        return Temp;

    }

    static void InternalGetFiles(List<String> lst, String strPath) {
        File dir = new File(strPath);
        File[] files = dir.listFiles(); // ���ļ�Ŀ¼���ļ�ȫ����������
        if (files != null) {
            for (int i = 0; i < files.length; i++) {
                String fileName = files[i].getName();
                if (files[i].isDirectory()) { // �ж����ļ������ļ���
                    InternalGetFiles(lst, files[i].getAbsolutePath()); // ��ȡ�ļ�����·��
                } else {
                    String strFileName = files[i].getAbsolutePath();
                    System.out.println("---" + strFileName);
                    lst.add(files[i].getAbsolutePath());
                }

            }

        }

    }

    public static String[] GetFiles(String strPath, boolean recursive) {
        if (!recursive)
            return GetFiles(strPath);
        else {
            List<String> arr = new ArrayList<String>();
            InternalGetFiles(arr, strPath);
            String[] strArr = new String[arr.size()];
            return arr.toArray(strArr);
        }

    }

    public static void Copy(String src,String dst) {


        FileChannel inputChannel = null;
        FileChannel outputChannel = null;
        try {
            File source ; File dest ;
            source = new File(src) ;
            dest = new File(dst) ;
            inputChannel = new java.io.FileInputStream(source).getChannel();
            outputChannel = new java.io.FileOutputStream(dest).getChannel();
            outputChannel.transferFrom(inputChannel, 0, inputChannel.size());
            inputChannel.close();
            outputChannel.close();
        } catch (Exception e) {
            e.printStackTrace();
        } finally {
            try {
                inputChannel.close();
                outputChannel.close();
            } catch (Exception ex) {
                ex.printStackTrace();
            }

        }
    }

    public static void CopyDir(String src, String des,int buffersize,Object Host,String ProcessingTaskMethodName)
    {

        //��ʼ���ļ�����
        File file1=new File(src);
        //���ļ��������ݷŽ�����
        File[] fs=file1.listFiles();
        //��ʼ���ļ�ճ��
        File file2=new File(des);
        //�ж��Ƿ�������ļ��в���û�д���
        if(!file2.exists()){
            file2.mkdirs();
        }
        //�����ļ����ļ���
        for (File f : fs) {
            if(f.isFile()){
                 Delegate del = new  Delegate() ;

                //�ļ�
                fileCopy(f.getPath(),des+"/"+f.getName(),buffersize,Host,del,ProcessingTaskMethodName); //�����ļ������ķ���
            }else if(f.isDirectory()){
                //�ļ���
                CopyDir(f.getPath(),des+"/"+f.getName(),buffersize,Host, ProcessingTaskMethodName);//�������ø��Ʒ���      �ݹ�ĵط�,�Լ������Լ��ķ���,�Ϳ��Ը����ļ��е��ļ�����
            }
        }




    }


    /**
     * �ļ����Ƶľ��巽��
     */
    private static void fileCopy(String src, String des, int buffersize, Object host, Delegate del, String copyCallbackMethodName)   {
        try
        {

            Method  mtd = null ;
             if(del != null)
             {
               mtd =  del.GetMethod("ExecuteTask") ;
             }
            java.io.File fsrc = new java.io.File(src) ;
            //io���̶���ʽ
            java.io.BufferedInputStream bis = new java.io.BufferedInputStream(new java.io.FileInputStream(src));
            java.io.BufferedOutputStream bos = new java.io.BufferedOutputStream(new java.io.FileOutputStream(des));
            int i = -1;//��¼��ȡ����
            byte[] bt = new byte[buffersize];//������
            while ((i = bis.read(bt))!=-1) {
                bos.write(bt, 0, i);
                if(mtd != null)
                    mtd.invoke(del, host,copyCallbackMethodName, fsrc.getName(),i,false ) ;
            }
            bis.close();
            bos.close();
            //�ر���

        }
        catch(Exception ex)
        {

        }

    }

    public static  void Move(String srcPath,String dstPath )
    {
        Rename(srcPath,dstPath);
    }
    public static  void Rename(String srcPath,String dstPath){
        try
        {
            File source ;
            source = new File(srcPath) ;
            source.renameTo(new File(dstPath)) ;
        }
        catch(Exception ex)
        {
            ex.printStackTrace();
        }



    }

}


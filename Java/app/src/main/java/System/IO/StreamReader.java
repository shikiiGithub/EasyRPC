package System.IO;

import  System.Text.Encoding;

import java.io.*;

/**
 * Created by shikii on 2019/4/30.
 */
public class StreamReader extends Stream {

    BufferedReader bf ;

    public StreamReader(String FilePath, Encoding en) {
        try {
            is  =  new FileInputStream(FilePath) ;
            rd = new InputStreamReader(is,en.toString());
            BufferedReader bf = new BufferedReader(rd );


        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    public String ReadLine()
    {
        // 按行读取字符串
        try {
            String str;
            str = bf.readLine()  ;
            return str ;
        } catch (IOException e) {
            e.printStackTrace();
            return null ;
        }
    }


    @Override
    protected void finalize() throws Throwable {
        bf.close();
        super.finalize();

    }
}

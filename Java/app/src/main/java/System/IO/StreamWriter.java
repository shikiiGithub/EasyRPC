package  System.IO;

import  System.Text.Encoding;

import java.io.*;


/**
 * Created by shikii on 2019/4/30.
 */
public class StreamWriter extends Stream {

    PrintStream ps ;

    public StreamWriter(String FilePath, Encoding en) {
        try {
            os  =  new PrintStream  (FilePath,en.toString()) ;
            ps = (PrintStream) os ;
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    public void WriteLine(String str)
    {

        try {

             ps.println(str); ;

        } catch ( Exception e) {
            e.printStackTrace();

        }
    }



}

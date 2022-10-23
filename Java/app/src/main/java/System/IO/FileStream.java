package System.IO;


import java.nio.channels.*;


/**
 * Created by shikii on 2019/4/30.
 */
public class FileStream extends  Stream {

    FileChannel fcl ;
    public FileStream(String FilePath) {
        try {
            is = new java.io.FileInputStream(FilePath) ;


        }
        catch (Exception e)
        {
            e.printStackTrace();
        }

        try
        {
            os =  new java.io.FileOutputStream(FilePath) ;
        }
        catch(Exception ex)
        {
            ex.printStackTrace();
        }

    }


    public void Flush()
    {
        try {
            ((java.io.FileOutputStream)os).flush();
        } catch ( Exception e) {
            e.printStackTrace();
        }
    }
    public long GetFileLen()
    {
        fcl = ((java.io.FileInputStream)is).getChannel() ;
        try {
            long size = fcl.size() ;
            return size ;
        } catch (Exception e) {
            e.printStackTrace();
            return Long.MIN_VALUE ;
        }
    }



}

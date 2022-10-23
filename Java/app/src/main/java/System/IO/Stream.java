package  System.IO;

import java.io.*;

/**
 * Created by shikii on 2019/4/30.
 */
public class Stream {

    //binary
    public InputStream is ;
    public OutputStream os ;
    //文本文件
    public Reader rd ;
    public Writer wt ;
    public void WriteByte(int nbyte)
    {
        try {
           os.write(nbyte);
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
    public void Write(byte [] bytes)
    {
        try {
            os.write(bytes);
        } catch (IOException e) {
            e.printStackTrace();

        }
    }

    public void Write(byte[] buffer, int offset, int count)
    {
        try {
            os.write(  buffer,   offset,  count);
        } catch (IOException e) {
            e.printStackTrace();

        }
    }
    public void Read(byte [] bytes)
    {
        try {
            is.read(bytes) ;
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
    public int ReadByte()
    {
        try {
           int n=   is.read() ;
            return n ;
        } catch (IOException e) {
            e.printStackTrace();
            return Integer.MIN_VALUE ;
        }
    }
    public void Read(byte [] bytes, int offset, int count)
    {
        try {
            is.read(  bytes,   offset,  count);
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    public void Close()
    {
        try {
            if(is != null)
            is.close();
            if(os != null)
            os.close();
            if(rd != null)
                rd.close();
            if(wt != null)
                wt.close();
        } catch (IOException e) {
            e.printStackTrace();
        }
    }

    @Override
    protected void finalize() throws Throwable {
        super.finalize();
        Close() ;
    }


}

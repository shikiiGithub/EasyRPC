package System.IO;

import java.io.BufferedReader;

/**
 * Created by shikii on 2019/4/30.
 */
public class StringReader {

   public java.io.StringReader sr ;

    public StringReader(String str)
    {
        sr = new java.io.StringReader(str) ;

    }

    public String ReadLine() {

        try {

            BufferedReader br = new BufferedReader(sr) ;
            String line = null ;
            return br.readLine() ;

        } catch ( Exception e1) {
            e1.printStackTrace();
            return null ;
        }
    }

    public void Close()
    {
        sr.close();
    }

    @Override
    protected void finalize() throws Throwable {
        super.finalize();
        Close() ;
    }
}

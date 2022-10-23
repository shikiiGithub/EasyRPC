package System.Text;


import java.io.UnsupportedEncodingException;

public class Encoding {
    String strEncode = null ;
    public Encoding()
    {

    }
    public Encoding(String str)
    {
        this.strEncode = str ;
    }
    public  static Encoding UTF8()
    {
        return  new Encoding("UTF8") ;
    }
    public  static Encoding ASCII()
    {

        return  new Encoding("US-ASCII") ;
    }

    public  static Encoding Unicode()
    {

        return  new Encoding("Unicode") ;
    }
    public  static Encoding Default()
    {

        return  new Encoding("GB2312") ;
    }
    public  static Encoding GBK()
    {

        return  new Encoding("GBK") ;
    }
    public String GetString(byte [] bytArr)
    {
        try {
            return new String(bytArr,0,bytArr.length,strEncode) ;
        } catch (UnsupportedEncodingException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
            return null ;
        }
    }
    public String ToString(byte [] bytes)
    {
        try {
            return new String(bytes,strEncode) ;
        } catch (UnsupportedEncodingException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
            return null ;
        }
    }
    public String GetString(byte [] bytArr, int nStart)
    {
        try {
            return new String(bytArr,nStart,bytArr.length-nStart,strEncode) ;
        } catch (UnsupportedEncodingException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
            return null ;
        }
    }
    public String GetString(byte [] bytArr, int nStart, int nLength)
    {
        try {
            return new String(bytArr,nStart,nLength,strEncode) ;
        } catch (UnsupportedEncodingException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
            return null ;
        }
    }
    /*
     * 123456789
     * 0123456789
     * */
    public String GetStringEx(byte [] bytArr, int nStart, int nEnd)
    {
        nEnd += 1 ;
        int nLength = nEnd - nStart ;
        try {
            return new String(bytArr,nStart,nLength,strEncode) ;
        } catch (UnsupportedEncodingException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
            return null ;
        }
    }

    public   byte [] GetBytes(String str)
    {
        try {
            return str.getBytes(strEncode) ;
        } catch (UnsupportedEncodingException e) {
            // TODO Auto-generated catch block
            e.printStackTrace();
            return null ;
        }


    }

    @Override
    public String toString() {
        return strEncode ;
    }
}

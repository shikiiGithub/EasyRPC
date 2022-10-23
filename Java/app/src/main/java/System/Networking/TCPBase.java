package System.Networking;

import System.Text.Encoding;
import  System.*;
public abstract class TCPBase
{
    protected boolean bEndNetwork =false ;
    protected int nPort = 8040;
    protected String strErrorInfo = null;

    protected Encoding en  ;
    protected Thread thd_Main;
    protected final byte SPLITMARK = 94;
    private String strIP;
    public static final byte MARKPOSITION = 5;

    public int BufferSize = 8192;

    public TCPBase() {
        en =  Encoding.UTF8();

    }

    public Encoding TextEncode()
    {
        return en;
    }
    public static void StoreDataLenByts(int nLen,
                                        byte[] buffer)
    {
        BitConverter.CopyTo(  BitConverter.GetBytes(
                nLen),buffer,1) ;
    }
    public static int FetchDataLenByts(byte[] buffer)
    {
        byte[] byt_Len = new byte[4];
        for (int i = 1; i < MARKPOSITION ; i++)
        {
            byt_Len[i - 1] = buffer[i];
        }
        int byteNum =  BitConverter.
                ToInt(byt_Len, 0
                );
        byt_Len = null;
        return byteNum;
    }
    public static void StoreMSGMark(byte[] buffer, byte byt)
    {
        buffer[0] = byt;
    }
    public static byte FetchMSGMark(byte[] buffer)
    {
        return buffer[0];
    }

    protected abstract void Loop()  ;
    protected abstract boolean Close();

    public byte[] ProvideBytes(String str)
    {

        return TextEncode().GetBytes(str);
    }
    public byte[] ProvideBytes(short srt)
    {
        return BitConverter.GetBytes(srt);
    }

    public byte[] ProvideBytes(int n)
    {
        return BitConverter.GetBytes(n);
    }
    //Need Convert byte array to a unsigned Num ?
    public Object ProvideNum(byte[] bytArr, int nStartIndex, int nBytesCount)
    {
        return BitConverter.ToObject(bytArr,nStartIndex,nBytesCount) ;
    }
    public String ProvideString(byte[] bytArr, int nIndex, int nCount)
    {

        return TextEncode().GetString(bytArr, nIndex, nCount);
    }

    public String getStrIP() {
        return strIP;
    }

    public void setStrIP(String strIP) {
        this.strIP = strIP;
    }

}

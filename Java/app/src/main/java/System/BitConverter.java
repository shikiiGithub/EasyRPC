package System;



//使用UTF8字符集
public  class BitConverter {


    public static  byte[] GetBytes(int n) {
        byte[] targets = new byte[4];
        targets[0] = (byte) (n & 0xff);// 最低位
        targets[1] = (byte) ((n >> 8) & 0xff);// 次低位
        targets[2] = (byte) ((n >> 16) & 0xff);// 次高位
        targets[3] = (byte) (n >>> 24);// 最高位,无符号右移。
        return targets;
    }
    public static byte[] GetBytes(short s) {
        byte[] targets = new byte[2];
        for (int i = 0; i < 2; i++) {
            int offset = (targets.length - 1 - i) * 8;
            targets[i] = (byte) ((s >>> offset) & 0xff);
        }
        return targets;
    }
    public static byte[] GetBytes(char data)
    {
        byte[] bytes = new byte[2];
        bytes[0] = (byte) (data);
        bytes[1] = (byte) (data >> 8);
        return bytes;
    }
    public static byte[] GetBytes(long data)
    {
        byte[] bytes = new byte[8];
        bytes[0] = (byte) (data & 0xff);
        bytes[1] = (byte) ((data >> 8) & 0xff);
        bytes[2] = (byte) ((data >> 16) & 0xff);
        bytes[3] = (byte) ((data >> 24) & 0xff);
        bytes[4] = (byte) ((data >> 32) & 0xff);
        bytes[5] = (byte) ((data >> 40) & 0xff);
        bytes[6] = (byte) ((data >> 48) & 0xff);
        bytes[7] = (byte) ((data >> 56) & 0xff);
        return bytes;
    }
    public static byte[] GetBytes(float data)
    {
        int intBits = Float.floatToIntBits(data);
        return GetBytes(intBits);
    }
    public static byte[] GetBytes(double data)
    {
        long intBits = Double.doubleToLongBits(data);
        return GetBytes(intBits);
    }

    public static  void CopyTo(byte [] bytArr_Src,byte [] bytArr_Dst,
                               int nIndexDstStart)
    {
        for (int i = 0; i < bytArr_Src.length; i++) {
            bytArr_Dst[i+nIndexDstStart]  = bytArr_Src[i] ;
        }
    }


    public  static int ToInt(byte[] buf, int startIndex) {
        int [] nArr = new int[4] ;
        int n = 0 ;
        int count = 4+startIndex;
        for (int i = startIndex; i < count ; i++) {
            if(buf[i]<0) {
                nArr[i-startIndex] = buf[i] &0xFF ;
            }
            else
                nArr[i-startIndex] = buf[i] ;
        }
        for (int i = 0; i < nArr.length; i++) {
            n = n | (nArr[i] & 0xFF)<<8*i ;
        }
        return  n ;
    }
    public static short ToShort(byte[] res,int nIndex_Start) {
        // res = InversionByte(res);
        // 一个byte数据左移24位变成0x??000000，再右移8位变成0x00??0000
        short targets = (short) ((res[nIndex_Start] & 0xff) | ((res[nIndex_Start+1 ] << 8) & 0xff00)); // | 表示安位或
        return targets;
    }
    public static char ToChar(byte[] bytes,int nIndexStart)
    {
        return (char) ((0xff & bytes[nIndexStart++]) | (0xff00 & (bytes[nIndexStart] << 8)));
    }
    public static long ToLong(byte[] bytes,int nIndexStart)
    {
        return(0xffL & (long)bytes[nIndexStart++]) | (0xff00L & ((long)bytes[nIndexStart++] << 8)) | (0xff0000L & ((long)bytes[nIndexStart++] << 16)) | (0xff000000L & ((long)bytes[nIndexStart++] << 24))
                | (0xff00000000L & ((long)bytes[nIndexStart++] << 32)) | (0xff0000000000L & ((long)bytes[nIndexStart++] << 40)) | (0xff000000000000L & ((long)bytes[nIndexStart++] << 48)) | (0xff00000000000000L & ((long)bytes[nIndexStart++] << 56));
    }
    public static float ToFloat(byte[] bytes,int nIndexStart)
    {
        return Float.intBitsToFloat(ToInt(bytes,nIndexStart));
    }
    public static double ToDouble(byte[] bytes,int nIndexStart)
    {
        long l = ToLong(bytes, nIndexStart);
        //System.out.println(l);
        return Double.longBitsToDouble(l);
    }
    public static Object ToObject(byte[] bytes, int nIndexStart, int nCount)
    {
        if(nCount==8)
            return (Object)ToDouble(bytes,nIndexStart) ;
        else if(nCount==4)
            return (Object) ToFloat(bytes,nIndexStart) ;
        else if(nCount == 2)
            return (Object) ToShort(bytes,nIndexStart) ;
        else
            return (Object) ToChar(bytes,nIndexStart) ;

    }
}
package System;

import java.text.ParseException;

public class Convert {
    public  static int ToInt(byte  byt )
    {
        if (byt < 0) {
            int n = byt & 0xFF;
            return n;
        } else {
            return byt ;
        }
    }
    public  static int ToInt(String str)
    {
        return Integer.parseInt(str) ;
    }
    public  static int ToInt(char str)
    {
        return  (int)str ;
    }
    public  static  float ToFloat(String str)
    {
        return Float.parseFloat(str) ;
    }
    public  static  double ToDouble(String str)
    {
        return Double.parseDouble(str) ;
    }
    public  static  long ToLong(String str)
    {
        return Long.parseLong(str) ;
    }
    public  static  boolean ToBoolean(String str)
    {
        return Boolean.parseBoolean(str) ;
    }
    public  static  long ToShort(String str)
    {
        return Long.parseLong(str) ;
    }
    public  static String ToString(int  Num)
    {
        return  String.valueOf(Num) ;
    }
    public  static String ToString(float  Num)
    {
        return  String.valueOf(Num) ;
    }public  static String ToString(short  Num)
    {
        return  String.valueOf(Num) ;
    }
    public  static String ToString(double  Num)
    {
        return  String.valueOf(Num) ;
    }
    public  static String ToString(long  Num)
    {
        return  String.valueOf(Num) ;
    }
    public  static String ToString(int  Num,char Format)
    {
        char ch = Character.toUpperCase(Format) ;
        switch (ch) {
            case 'X':
                return Integer.toHexString(Num) ;

            case 'O':

                return Integer.toOctalString(Num) ;

            case 'B':

                return Integer.toBinaryString(Num) ;


        }
        return null ;
    }
    public  static String ToString(byte  Num,char Format)
    {
        char ch = Character.toUpperCase(Format) ;
        switch (ch) {
            case 'X':
                if (Num < 0) {
                    int n = Num & 0xFF;
                    return Integer.toHexString(n);
                } else {
                    return Integer.toHexString(Num) ;
                }
            case 'O':
                if (Num < 0) {
                    int n = Num & 0xFF;
                    return Integer.toOctalString(n);
                } else {
                    return Integer.toOctalString(Num) ;
                }
            case 'B':
                if (Num < 0) {
                    int n = Num & 0xFF;
                    return Integer.toBinaryString(n);
                } else {
                    return Integer.toBinaryString(Num) ;
                }

        }
        return null ;
    }
    public static String ToString(byte [] bytes,char Format)
    {
        char ch = Character.toUpperCase(Format) ;
        StringBuilder sb = new StringBuilder() ;
        switch (ch) {
            case 'X':
                for (int i = 0; i < bytes.length; i++) {
                    if(bytes[i]<0)
                    {
                        sb.append(  Integer.toHexString(bytes[i]&0xFF)) ;
                    }
                    else
                        sb.append(   Integer.toHexString(bytes[i])) ;
                }
                break;
            case 'O':
                for (int i = 0; i < bytes.length; i++) {
                    if(bytes[i]<0)
                    {
                        sb.append(  Integer.toOctalString(bytes[i]&0xFF)) ;
                    }
                    else
                        sb.append(   Integer.toOctalString(bytes[i])) ;
                }
                break;
            case 'B':
                for (int i = 0; i < bytes.length; i++) {
                    if(bytes[i]<0)
                    {
                        sb.append(  Integer.toBinaryString(bytes[i]&0xFF)) ;
                    }
                    else
                        sb.append(   Integer.toBinaryString(bytes[i])) ;
                }
                break;

        }
        return sb.toString() ;
    }
    public static DateTime ToDateTime(String str)
    {

        return DateTime.ParseDateTime(str) ;

    }

}

package System;

import java.util.Scanner;

public class Console
{
    static Scanner scn= null;
    public static  void WriteLine(String str)
    {
        System.out.println(str);
    }

    public static  void Write(String str)
    {
        System.out.print(str);
    }
    public static  void Write(int  Num)
    {
        System.out.print(Num);
    }
    public static  void Write(short  Num)
    {
        System.out.print(Num);
    }
    public static  void Write(char  Num)
    {
        System.out.print(Num);
    }
    public static  void Write(long  Num)
    {
        System.out.print(Num);
    }
    public static  void Write(float  Num)
    {
        System.out.print(Num);
    }
    public static  void Write(double  Num)
    {
        System.out.print(Num);
    }

    public static  void WriteLine(int  Num)
    {
        System.out.println(Num);
    }
    public static  void WriteLine(short  Num)
    {
        System.out.println(Num);
    }
    public static  void WriteLine(char  Num)
    {
        System.out.println(Num);
    }
    public static  void WriteLine(long  Num)
    {
        System.out.println(Num);
    }
    public static  void WriteLine(float  Num)
    {
        System.out.println(Num);
    }
    public static  void WriteLine(double  Num)
    {
        System.out.println(Num);
    }
    public static  char ReadKey()
    {
        if(scn==null )
            scn = new Scanner(System.in);
        return  (char)scn.nextByte() ;
    }
    public static String ReadLine()
    {

        if(scn==null)
            scn = new Scanner(System.in);
        return (String) scn.nextLine() ;
    }

}
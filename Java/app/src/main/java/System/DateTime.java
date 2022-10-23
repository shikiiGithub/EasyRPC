package System;
import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;

public class DateTime
{
    public int Year ,Month ,Day,Hour,Min,Secs ;
    //从1970-1-1的秒数
    public long  LotsSecs ;
    Date date ;

    public   DateTime()
    {

        date = new Date() ;
    }


    public DateTime(Date dt)
    {
        this.date = dt ;
    }

    public String ToDateTime()
    {
        SimpleDateFormat df = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
        return  df.format(date) ;
    }

    public String ToDate()
    {
        SimpleDateFormat df = new SimpleDateFormat("yyyy-MM-dd");
        return  df.format(date) ;
    }

    public String ToShortTimeString()
    {
        SimpleDateFormat df = new SimpleDateFormat("HH:mm");
        return  df.format(date) ;
    }

    public String ToLongTimeString()
    {
        SimpleDateFormat df = new SimpleDateFormat("HH:mm:ss");
        return  df.format(date) ;
    }

    public long GetSecsFrom1970_1_1()
    {
        this.LotsSecs =  System.currentTimeMillis() ;
        return LotsSecs ;
    }

    public static  DateTime Now()
    {
        return new DateTime() ;
    }

    public static  long GetTicks()
    {
        long milli = System.currentTimeMillis() ;//+ 8*3600*1000;
        long ticks = (milli*10000)+621355968000000000L;
        return ticks ;
    }

    public static  DateTime ParseDateTime(String str)
    {
        SimpleDateFormat sdf = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
        try {

            Date date = sdf.parse(str) ;
            return new DateTime(date) ;

        } catch (ParseException e) {
            return null ;
        }

    }

}
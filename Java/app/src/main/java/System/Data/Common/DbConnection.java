package System.Data.Common;

/**
 * Created by shikii on 2018/4/22.
 */
public abstract class DbConnection {
   public String  ConnectionString ;
   public String  DataSource ;
   public String  Database ;
    public Object DBObject ;
    //尚未实现

    public abstract boolean Open()  ;
    public abstract boolean Close() ;
    public abstract boolean	Dispose() ;
    public String ToString()
    {
          return ConnectionString ;
    }
    public DbCommand CreateCommand ()
    {
        return null ;
    }
}

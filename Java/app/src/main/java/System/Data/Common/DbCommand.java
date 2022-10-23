package System.Data.Common;



/**
 * Created by shikii on 2018/4/22.
 */
public abstract class DbCommand
{
   public String CommandText ;
    public DbConnection Connection ;
   // public 	Parameters
    public abstract void ExecuteNonQuery	() ;
    public abstract void ExecuteNonQuery	(Object... BinArgs) ;
    public abstract DataTable ExecuteReader();// 针对 Connection 执行 CommandText，并返回 DbDataReader
    public abstract Object ExecuteScalar() ;	//执行查询，并返回查询所返回的结果集中第一行的第一列。所有其他的列和行将被忽略。
    public void  Dispose (){

        try {
            finalize();
        } catch (Throwable throwable) {
            throwable.printStackTrace();
        }
    }
     /* public Object CreateParameter()
    {
        return null ;
    }*/
}

package System.Data.Common;

import java.util.ArrayList;
import java.util.List;

/**
 * Created by shikii on 2018/4/22.
 */
public   class DataTable {
    public List<Object> LinearDataCollection;

    public int Columns = 0;
    public int Rows = 0;
    public DataTable()
    {
        this.LinearDataCollection = new ArrayList<Object>();
    }
    public void Dispose() {

        LinearDataCollection.clear();
        try {
            this.finalize();
        } catch (Throwable throwable) {
            throwable.printStackTrace();
        }
    }
    public <T> T IndexOf(int nIndex_Row, int nIndex_Col) {
        if (nIndex_Row * Columns + nIndex_Col >= LinearDataCollection.size())
            return null;
        else if (nIndex_Row * Columns + nIndex_Col < 0)
            return null;
        else
            return (T)LinearDataCollection.get(nIndex_Row * Columns + nIndex_Col) ;
    }
}

package System.Collections.Generic;
import java.lang.reflect.Array;
import java.util.ArrayList;

/**
 * Created by Shikii on 2017/12/13.
 */
public class List<T>  {
    public java.util.List<T> lst = null ;

    public List()
    {
        lst = new ArrayList<T>() ;

    }
    public List(java.util.List<T> lst)
    {
        this.lst = lst ;
    }
    public void Add(T item)
    {
        lst.add(item) ;
    }
    public void AddRange(T... arr)
    {
        for (int i = 0; i < arr.length; i++) {
            lst.add(arr[i]) ;
        }
    }
    public void Clear()
    {
        lst.clear();
    }
    public int Count()
    {

        return lst.size() ;
    }
    public T Item(int nIndex)
    {
        return  lst.get(nIndex) ;
    }
    public void Item(int nIndex,T value)
    {
        lst.set(nIndex,value) ;
    }
    public boolean Contains(Object o)
    {
        return  lst.contains(o) ;
    }
    public int IndexOf(Object t)
    {
        return lst.indexOf(t) ;
    }
    public void Insert(int nIndex,T t)
    {
        lst.add(nIndex,t); ;
    }
    public void Remove(T t)
    {
        lst.remove(t) ;
    }
    public void RemoveAt(int n)
    {
        lst.remove(n) ;
    }
    public int LastIndexOf(Object o)
    {

        return lst.lastIndexOf(o) ;
    }
    public T[] ToArray()
    {
        if(lst.size()>0) {
            Class  cls = lst.get(0).getClass() ;
            T[] ts = (T[]) Array.newInstance(cls, lst.size());
            for (int i = 0; i < ts.length; i++) {
                ts[i] = lst.get(i) ;
            }
            return ts ;
        }
        else
            return null ;
    }
    public void Reverse()
    {
        if(lst.size()>0) {
            Class  cls = lst.get(0).getClass() ;
            T[] ts = (T[]) Array.newInstance(cls, lst.size());
            for (int i = 0; i < ts.length; i++) {
                ts[i] = lst.get(i) ;
            }
            lst.clear();
            for (int i = ts.length-1; i >-1; i--) {
                lst.add(ts[i]) ;
            }
        }
    }

}

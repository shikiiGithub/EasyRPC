package System.Collections.Generic;

import java.util.ArrayList;
import java.util.Collection;
import java.util.HashMap;
import java.util.Set;

public class Dictionary<K,V> {
    HashMap<K,V> _dic = new HashMap<>();

    public void Add(K key,V val){
        _dic.put(key,val);
    }

    public V GetValue(K key)
    {
       return  _dic.get(key);
    }

    public void Replace(K key, V val)
    {
        _dic.replace(key,val);
    }

    public boolean Contains(K key)
    {
        return _dic.containsKey(key);
    }
    public boolean ContainsValue(V value)
    {
        return _dic.containsValue(value);
    }

    public int Count()
    {
        return _dic.size();
    }

    public  void Clear()
    {
        _dic.clear();
    }

    public void Remove(K key)
    {
        _dic.remove(key);
    }
    public  List<K> Keys()
    {
        Set<K> cols  = _dic.keySet() ;
        List<K> lst = new List<>();
        cols.forEach( t->lst.Add(t));
        return lst ;
    }
    public  List<V> Values()
    {
        Collection<V> cols  = _dic.values();
        List<V> lst = new List<>();
        cols.forEach( t->lst.Add(t));
        return lst ;
    }
}

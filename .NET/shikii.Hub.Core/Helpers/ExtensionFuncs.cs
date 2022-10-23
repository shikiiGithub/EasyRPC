using LitJson;
//using Serilog;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
 

namespace shikii.Hub.Common
{
    public  static partial class ExtensionFuncs
    {
         public static Object ThisDiManager = null ;
         static Assembly WinFormAssembly = null;

        //public static void InfoEx(this ILogger logger, String text, DbPlatformBase dbPlatform)
        //{

        //    logger.Information(text);
        //    String level = "INFO";
        //    text = text.Replace("'", "''");
        //    dbPlatform?.NewRecord("AppLogs", String.Format("'{0}','{1}','{2}'", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), level, text));

        //}

        //public static void ErrorEx(this ILogger logger, String text, DbPlatformBase dbPlatform)
        //{
            
        //    logger.Error(text);
        //    String level = "ERROR";
        //    text = text.Replace("'", "''");
        //    dbPlatform?.NewRecord("AppLogs", String.Format("'{0}','{1}','{2}'", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), level,  text));

        //}

        //public static void ErrorEx(this ILogger logger, Exception e, DbPlatformBase dbPlatform)
        //{

        //    String text = e.Message + " " + e.StackTrace;
        //    logger.Error(e, "Error {Message} records.", e.Message); ;
        //    String level = "ERROR";
        //    text = text.Replace("'", "''");
        //    dbPlatform?.NewRecord("AppLogs", String.Format("'{0}','{1}','{2}'", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), level, text));

        //}

        public static void TipError(String text)
        {
            AssemblyName[] asms = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            asms.ToList().ForEach(x =>
            {
                if (x.Name.Contains("System.Windows.Forms"))
                {
                    String local = x.FullName;
                    if(WinFormAssembly == null)
                        WinFormAssembly = Assembly.Load(x);
                    Type type = WinFormAssembly.GetType("System.Windows.Forms.MessageBox");
                    MethodInfo[] mif = type.GetMethods();
                    mif[11].Invoke(null, new Object[] {text, "提示",0,16  });
                }
            });
        }

        public static void TipAsk(String text)
        {
            AssemblyName[] asms = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            asms.ToList().ForEach(x =>
            {
                if (x.Name.Contains("System.Windows.Forms"))
                {
                    String local = x.FullName;
                    if (WinFormAssembly == null)
                        WinFormAssembly = Assembly.Load(x);
                    Type type = WinFormAssembly.GetType("System.Windows.Forms.MessageBox");
                    MethodInfo[] mif = type.GetMethods();
                    mif[11].Invoke(null, new Object[] { text, "询问", 0, 32 });
                }
            });
        }
        public static void TipInfo(String text)
        {
            AssemblyName[] asms = Assembly.GetExecutingAssembly().GetReferencedAssemblies();
            asms.ToList().ForEach(x =>
            {
                if (x.Name.Contains("System.Windows.Forms"))
                {
                    String local = x.FullName;
                    if (WinFormAssembly == null)
                        WinFormAssembly = Assembly.Load(x);
                    Type type = WinFormAssembly.GetType("System.Windows.Forms.MessageBox");
                    MethodInfo[] mif = type.GetMethods();
                    
                    mif[11].Invoke(null, new Object[] { text, "消息", 0, 64 });
                }
            });
        }
           
        public static T Val<T>(this LitJson.JsonData jsonData,String key) 
        {
            String baseTypeName = typeof(T).BaseType.Name;
            String typeName = typeof(T).Name; 
            Func<JsonData,T> GetValue = (_jsonData) =>
            {
                LitJson.IJsonWrapper tmp = (_jsonData as LitJson.IJsonWrapper);
           
                switch (baseTypeName)
                {
                    case "Object":
                        if (typeName == "String")
                        {
                            return (T)(Object)tmp.GetString();
                        }
                        else if (typeName == "List`1")
                            goto cc;
                        break;

                    case "ValueType":
                        
                        switch(typeName)
                        {
                            case "Int32": return (T)(Object)tmp.GetInt();
                            case "Int64": return (T)(Object)tmp.GetLong();
                            case "Double": return (T)(Object)tmp.GetDouble();
                            case "Boolean": return (T)(Object)tmp.GetBoolean();
                            case "Single": return (T)_jsonData.GetType().GetMethod("GetSingle").Invoke(_jsonData, null);
                        }
                        break;

                    case "Array":
                    cc:;
                        //T arr =(T) System.Activator.CreateInstance(typeof(T)) ;
                        //IList<T> lst = arr as IList<T>;

                        if (tmp.IsArray)
                        {
                           IList lst = tmp  ;
                           
                        }
                        //else 

                        // IDictionaryEnumerator enu  = tmp.GetEnumerator();
                        // int n = 0;
                        //while (enu.MoveNext())
                        //{
                            
                        //}

                        break;

                }
                throw new Exception("Type 不匹配.type name:"+typeName);
                return default(T);
                
            };

            if(key.Contains("."))
            {
                String [] str = key.Split('.');
                String [] Arr = str.Where(x => !String.IsNullOrEmpty(x.Trim().Replace(" ",""))).ToArray();
                JsonData tmp = jsonData;

                Arr.ToList().ForEach(x =>
                {
                    if (tmp.ContainsKey(x))
                        tmp = tmp[x];
                    else
                        throw new Exception("键名不匹配");
                    
                });

             return    GetValue(tmp);

            }
            else if(jsonData.Keys.Contains(key))
            {

                return GetValue(jsonData[key]);
            }
                
            else
            {
                throw new Exception("键名不匹配");
                return default(T);
            }
          



        }

        /// <summary>
        /// 仅适用于集合中元素为引用类型，返回为空
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array"></param>
        /// <param name="Predicate"></param>
        public static void SelectEx<T>(this IEnumerable<T> array, Action<T> Predicate)
        {

            IEnumerator en = array.GetEnumerator();
            while (en.MoveNext())
            {
                Predicate((T)en.Current);

            }

        }
        public static void AllExecute<T>(this IEnumerable<T> array, Action<T> Predicate)
        {
            int n = array.Count();
            if (n > 0)
            {
                IEnumerator en = array.GetEnumerator();
                while (en.MoveNext())
                {
                    Predicate((T)en.Current);

                }

            }

        }

        public static void AllExecute(this IEnumerable array, Action<Object> Predicate)
        {
             
                IEnumerator en = array.GetEnumerator();
                while (en.MoveNext())
                {
                    Predicate(en.Current);

                }

           

        }

        public static int IndexOf<T>(this Array array, T obj)
        {
            return Array.IndexOf<T>((T[])array, obj);
        }
        public static T GetCustomAttribute<T>(this PropertyInfo pif) where T : Attribute
        {


            Type AttributeType = typeof(T);

            Object temp = Attribute.GetCustomAttribute(pif, AttributeType);

            if (temp != null)
                return (T)temp;
            else
                return null;
        }

        public static T GetCustomAttribute<T>(this Type type) where T : Attribute
        {
            Object[] objs = type.GetCustomAttributes(true);

            foreach (var item in objs)
            {
                Object obj = item as T;
                if (obj != null)
                    return (T)item;
            }

            return null;

        }

        public static T[] GetCustomAttributes<T>(this Type type) where T : Attribute
        {
            
            Object[] objs = type.GetCustomAttributes(true);
            if (objs == null || objs.Length < 1)
                return null;
            List<T> lst = new List<T>();
            
            int n = 0;
            foreach (var item in objs)
            {
               T t = item as T;
                if (t != null)
                    lst.Add(t);

            }

            return lst.ToArray();

        }

        /// <summary>
        /// 可用于分页，分割数组
        /// </summary>
        /// <typeparam name="T">元素类型</typeparam>
        /// <param name="ElementAmountPerBlock">每个块的元素数量</param>
        /// <param name="BlockIndex">块的索引</param>
        /// <returns>数组</returns>
        public static T[] ToIndexArray<T>(this IEnumerable<T> ts,
               int ElementAmountPerBlock, int BlockIndex)
        {
            int SourceAmount = ts.Count();
            if (SourceAmount == 0)
                return null;

            //总页数

            int parts = SourceAmount / ElementAmountPerBlock;
            if (SourceAmount % ElementAmountPerBlock != 0)
                parts += 1;
            if (BlockIndex >= parts)
                BlockIndex = parts - 1;

            if (BlockIndex <= 0)
                BlockIndex = 0;


            int nBegineIndex = ElementAmountPerBlock * BlockIndex;
            int ncount = 0;

            if (0 == BlockIndex && parts == 1)
            {
                if (SourceAmount < ElementAmountPerBlock)
                    ncount = SourceAmount;

            }
            else if (BlockIndex == parts - 1)
            {
                if (SourceAmount < ElementAmountPerBlock * parts)
                {
                    ncount = SourceAmount - (parts - 1) * ElementAmountPerBlock;
                }
                else
                    ncount = ElementAmountPerBlock;
            }

            else
            {
                ncount = ElementAmountPerBlock;
            }



            T[] arr = new T[ncount];

            T[] src = ts.ToArray();

            for (int i = nBegineIndex; i < nBegineIndex + ncount; i++)
            {
                arr[i - nBegineIndex] = src[i];
            }
            return arr;

        }


        /// <summary>
        /// 截取字符串
        /// </summary>
        /// <param name="s"></param>
        /// <param name="index">首索引</param>
        /// <param name="end">终索引</param>
        /// <param name="ExceptEndIndex">终索引是否包含在内（默认包含）</param>
        /// <returns></returns>
        public static String SubString(this String s, long index, long end, bool ExceptEndIndex = false)
        {
            if (ExceptEndIndex)
                return s.Substring((int)index, (int)(end - index + 1));
            else
                return s.Substring((int)index, (int)(end - index));

        }

       

        /// <summary>
        /// 是否不为null 或empty
        /// </summary>
        public static bool IsValideString(this String str, bool CheckWhiteSpace = false)
        {
            return !String.IsNullOrEmpty(str) && !String.IsNullOrWhiteSpace(str);
        }

        public static String ConnectAll<T>(this IEnumerable<T> array, String gapStr)
        {
            int n = array.Count();
            StringBuilder sb = new StringBuilder();
            if (n > 0)
            {
                IEnumerator en = array.GetEnumerator();
                while (en.MoveNext())
                {
                    String str = en.Current.ToString();
                    sb.AppendFormat("{0}{1}", str, gapStr);
                }
                String s = sb.ToString();
                int nindex = s.LastIndexOf(gapStr);
                s.Substring(0, nindex);
                return s;
            }
            return null;
        }

        public static List<T> FirstColumn<T>(this DataTable dt)
        {
            List<T> lst = new List<T>();
            if (dt == null)
                return lst;
            if (dt.Rows.Count > 0)
            {
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {
                        lst.Add((T)dt.Rows[i][j]);
                    }

                }
            }
            return lst;
        }

        //C:\Users\shikii\Desktop\CFGS -> CFGS
        //C:\Users\shikii\Desktop\CFGS\a.txt -> a.txt
        public static String[] GetShortNames(this IEnumerable<String> array)
        {
            int n = array.Count();
            if (n == 0)
                return null;

            String[] arr = new string[n];
            int i = 0;

            IEnumerator en = array.GetEnumerator();
            while (en.MoveNext())
            {
                String str = en.Current.ToString();
                arr[i++] = Path.GetFileName(str);
            }


            return arr;
        }
        //C:\Users\shikii\Desktop\CFGS\a.txt -> C:\Users\shikii\Desktop\CFGS
        //removeStr要移除的字符串
        //newReplaceStr 替代的字符串
        public static String[] GetDirs(this IEnumerable<String> array, String removeStr = null, String newReplaceStr = null)
        {
            int n = array.Count();
            if (n == 0)
                return null;

            String[] arr = new string[n];
            int i = 0;

            IEnumerator en = array.GetEnumerator();
            while (en.MoveNext())
            {
                String str = en.Current.ToString();
                if (removeStr.IsValideString())
                    arr[i++] = Path.GetDirectoryName(str).Replace(removeStr, newReplaceStr);
            }


            return arr;
        }

        public static int ToInt(this String str)
        {

            try
            {
                return int.Parse(str);
            }
            catch (Exception ex)
            {

                throw;

            }
        }
        public static float ToFloat(this String str)
        {

            try
            {
                return float.Parse(str);
            }
            catch (Exception ex)
            {

                throw;

            }
        }
        public static double ToDouble(this String str)
        {

            try
            {
                return double.Parse(str);
            }
            catch (Exception ex)
            {

                throw;

            }
        }

    }

   
}
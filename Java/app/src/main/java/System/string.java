package System;
/**
 * Created by Shikii on 2017/12/9.
 */
public class string {
    StringBuilder sb = null;

    @Override
    protected void finalize() throws Throwable {
        Clear();
    }

    @Override
    public String toString() {
        return ToString();
    }


    public static string Instance() {
        return new string();
    }

    public static string Instance(CharSequence cs) {
        return new string(cs);
    }

    public static string Instance(String cs) {
        return new string(cs);
    }

    public boolean Contains(String value) {
        int n = sb.indexOf(value);
        if (n == -1)
            return false;
        else
            return true;
    }

    public string(StringBuilder sb) {
        this.sb = sb;

    }
    //list 去重
    public static void UniqueList( java.util.List list)
    {
        for  ( int  i  =   0 ; i  <  list.size()  -   1 ; i ++ )  {
            for  ( int  j  =  list.size()  -   1 ; j  >  i; j -- )  {
                if  (list.get(j).equals(list.get(i)))  {
                    list.remove(j);
                }
            }
        }
    }

    public string(String str) {
        Clear();
        if (sb == null) {
            sb = new StringBuilder(str);
        } else {
            sb.append(str);
        }
    }

    public string(CharSequence cs) {
        Clear();
        if (sb == null) {
            sb = new StringBuilder(cs);
        } else {
            sb.append(cs);
        }
    }

    public string(char[] chs) {
        Clear();
        if (sb == null) {
            sb = new StringBuilder();
            sb.append(chs);
        } else {
            sb.append(chs);
        }
    }

    public string() {
        sb = new StringBuilder();
    }

    public void Clear() {
        if (sb != null) {
            sb.delete(0, sb.length());
        }
    }

    //找不到返回-1
    public int IndexOf(String str) {
        return sb.indexOf(str);
    }

    public int LastIndexOf(String str) {
        return sb.lastIndexOf(str);
    }

    public String SubString(int nstart) {
        return sb.substring(nstart);
    }

    //[nStart,nEndIndex]
    public String SubString(int nStartIndex, int nEndIndex) {

        return sb.substring(nStartIndex, nEndIndex + 1);
    }

    public boolean Equals(Object o) {
        return sb.equals(o);
    }

    public int Length() {
        return sb.length();
    }

    public void Assign(String str) {
        Clear();
        if (sb == null) {
            sb = new StringBuilder(str);
        } else {
            sb.append(str);
        }
    }

    public void Append(char ch) {
        sb.append(ch);
    }

    public void Append(String s) {
        sb.append(s);
    }

    public void Append(CharSequence s) {
        sb.append(s);
    }

    public static boolean IsNullOrEmpty(String value) {
        if (value == null || value.equals(""))
            return true;
        else
            return false;

    }

    public static boolean IsNullOrEmptyOrSpace(String value) {
        if (value == null || value.equals("") || value.equals(" "))
            return true;
        else
            return false;
    }

    //移除当前实例中的所有字符，从指定位置开始，一直到最后一个位置为止，并返回字符串。
    public String Remove(int startIndex) {
        sb.delete(startIndex, sb.length());
        return sb.toString();
    }

    public String RemoveByInterval(int startIndex, int nEndIndex) {
        sb.delete(startIndex, nEndIndex + 1);
        return sb.toString();
    }

    public String Remove(int startIndex, int count) {
        sb.delete(startIndex, startIndex + count);
        return sb.toString();
    }

    public String Replace(char oldChar, char newChar) {
        return sb.toString().replace(oldChar, newChar);
    }

    public String Replace(String oldValue, String newValue) {
        return sb.toString().replace(oldValue, newValue);
    }

    //可以使用正则表达式(默认)
    public String[] Split(String separator) {
        return sb.toString().split(separator);
    }

    public String ToLower() {
        return sb.toString().toLowerCase();
    }

    public String ToUpper() {
        return sb.toString().toUpperCase();
    }

    public String Trim() {
        return sb.toString().trim();
    }

    public void AppendFormat(String strFormat, Object... objArr) {
        sb.append(String.format(strFormat, objArr));
    }

    public void Insert(int nStartIndex, String strValue) {
        sb.insert(nStartIndex, strValue);
    }

    public static String FormatS(String strFormat, Object... objArr) {
        return String.format(strFormat, objArr);
    }

    public static string Formats(String strFormat, Object... objArr) {
        return new string(String.format(strFormat, objArr));
    }

    public static void ClearStringBuilder(StringBuilder sb) {
        sb.delete(0,sb.length()) ;
    }

    public static void AppendLine(StringBuilder sb,String line)
    {
        sb.append(line) ;
        sb.append(Environment.NewLine()) ;
    }

    public static    String Trim(String src, char... trimChars)
    {
        if (trimChars == null || trimChars.length == 0)
        {
            return TrimHelper(  src, 2);
        }
        return TrimHelper( src, trimChars, 2);
    }


    public static String TrimStart( String src,  char ... trimChars)
    {
        if (trimChars == null || trimChars.length == 0)
        {
            return TrimHelper( src, 0);
        }
        return TrimHelper( src, trimChars, 0);
    }


    public static String TrimEnd( String src,  char... trimChars)
    {
        if (trimChars == null || trimChars.length == 0)
        {
            return TrimHelper( src, 1);
        }
        return TrimHelper(src,trimChars, 1);
    }
    private static String TrimHelper(String src,char[] trimChars, int trimType)
    {
        int num = src.length() - 1;
        int i = 0;
        if (trimType != 1)
        {
            for (i = 0; i < src.length(); i++)
            {
                int num2 = 0;
                char c = src.charAt(i);
                for (num2 = 0; num2 < trimChars .length  && trimChars[num2] != c; num2++)
                {
                }
                if (num2 == trimChars .length )
                {
                    break;
                }
            }
        }
        if (trimType != 0)
        {
            for (num = src.length() - 1; num >= i; num--)
            {
                int num3 = 0;
                char c2 = src.charAt(num);
                for (num3 = 0; num3 < trimChars .length && trimChars[num3] != c2; num3++)
                {
                }
                if (num3 == trimChars.length)
                {
                    break;
                }
            }
        }
        return CreateTrimmedString(src,i, num);
    }

    private static String TrimHelper(String src,int trimType)
    {
        int num = src.length()  - 1;
        int i = 0;
        if (trimType != 1)
        {

            for (i = 0; i < src.length()  && (Character.isWhitespace(src.charAt(i))  ); i++)
            {
            }
        }
        if (trimType != 0)
        {
            num = src.length() - 1;
            while (num >= i && (Character.isWhitespace(src.charAt(i))))
            {
                num--;
            }
        }
        return CreateTrimmedString(src,i, num);
    }

    private static String CreateTrimmedString(String src,int start, int end)
    {
        int num = end - start + 1;
        if (num == src.length() )
        {
            return src;
        }
        if (num == 0)
        {
            return "";
        }
        return src.substring(start,start+num);
    }
    public String ToString()
    {
        return sb.toString() ;
    }

}


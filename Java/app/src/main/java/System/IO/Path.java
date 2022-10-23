package System.IO;

import java.io.File;

public class Path {
    public static final char DirectorySeparatorChar = '\\';

    final String DirectorySeparatorCharAsString = "\\";


    public static final char AltDirectorySeparatorChar = '/';


    public static final char VolumeSeparatorChar = ':';

    public static String GetFileName(String path)
    {
         File f = new File(path) ;
          return f.getName()  ;
    }
    public static String GetDirectoryName(String path){
        File f = new File(path) ;
        return f.getParent() ;
    }

    public static String GetExtension(String path)
    {
        File f = new File(path) ;
       String str = f.getName() ;
         int nDotLocation = str.lastIndexOf(".") ;
      return   str.substring(nDotLocation+1) ;

    }


    public static String GetFileNameWithoutExtension(String path)
    {
        try
        {
            File f = new File(path) ;
            String str = f.getName() ;
            int nDotLocation = str.lastIndexOf(".") ;
            if(nDotLocation != -1)
              str = str.substring( 0,nDotLocation) ;


            return str ;
        }
        catch(Exception ex)
        {
            ex.printStackTrace();
            return null ;
        }

    }


    public static String Combine(String... paths){
        StringBuilder sb =new StringBuilder() ;
        for (int i = 0; i < paths.length; i++) {
           sb.append( String.format("%s/",paths[i]) );
        }
        sb.deleteCharAt(sb.length()-1) ;
        return sb.toString() ;
    }
    public static String RemoveLastSpliter(String strPath)
    {
           int n = strPath.charAt(strPath.length()-1) ;
           if(n=='/')
           {
        	  return strPath.substring(0, strPath.length()) ;
           }
           else {
			return strPath ;
		}
        
    }
    public static String ChangeExtension(String path, String extension)
    {
        if (path != null)
        {

            String text = path;
            int num = path.length();
            while (--num >= 0)
            {
                char c = path.charAt(num);
                if (c == '.')
                {
                    text = path.substring(0, num);
                    break;
                }
                if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar || c == VolumeSeparatorChar)
                {
                    break;
                }
            }
            if (extension != null && path.length() != 0)
            {
                if (extension.length()  == 0 || extension.charAt(0) != '.')
                {
                    text += ".";
                }
                text += extension;
            }
            return text;
        }
        return null;
    }
    public static boolean HasExtension(String path)
    {
        if (path != null)
        {

            int num = path.length();
            while (--num >= 0)
            {
                char c = path.charAt(num);
                if (c == '.')
                {
                    if (num != path.length() - 1)
                    {
                        return true;
                    }
                    return false;
                }
                if (c == DirectorySeparatorChar || c == AltDirectorySeparatorChar || c == VolumeSeparatorChar)
                {
                    break;
                }
            }
        }
        return false;
    }


}

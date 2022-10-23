package System.Web;

import  System.Text.Encoding;

public class HttpUtility {
    static final  String allowChars = ".!*'();:@&=+_\\-$,/?#\\[\\]{}|\\^~`<>%\"";
    public  static  String UrlEncode(String rawString, Encoding en)
    {

        try {
            if(!needEncoding(rawString))
                return rawString ;
            else {
                String temp =  Encode(rawString, en.toString(), allowChars, false) ;
                return temp ;
            }
        } catch ( Exception e) {
            e.printStackTrace();
            return null ;
        }
    }

    static   String Encode(String s, String enc, String allowed,
                           boolean lowerCase)  {

        try
        {
            byte[] bytes = s.getBytes(enc);
            int count = bytes.length;

            /*
             * From RFC 2396:
             *
             * mark = "-" | "_" | "." | "!" | "~" | "*" | "'" | "(" | ")" reserved =
             * ";" | "/" | ":" | "?" | "@" | "&" | "=" | "+" | "$" | ","
             */
            // final String allowed = "=,+;.'-@&/$_()!~*:"; // '?' is omitted
            char[] buf = new char[3 * count];
            int j = 0;

            for (int i = 0; i < count; i++) {
                if ((bytes[i] >= 0x61 && bytes[i] <= 0x7A) || // a..z
                        (bytes[i] >= 0x41 && bytes[i] <= 0x5A) || // A..Z
                        (bytes[i] >= 0x30 && bytes[i] <= 0x39) || // 0..9
                        (allowed.indexOf(bytes[i]) >= 0)) {
                    buf[j++] = (char) bytes[i];
                } else {
                    buf[j++] = '%';
                    if (lowerCase) {
                        buf[j++] = Character.forDigit(0xF & (bytes[i] >>> 4), 16);
                        buf[j++] = Character.forDigit(0xF & bytes[i], 16);
                    } else {
                        buf[j++] = lowerCaseToUpperCase(Character.forDigit(
                                0xF & (bytes[i] >>> 4), 16));
                        buf[j++] = lowerCaseToUpperCase(Character.forDigit(
                                0xF & bytes[i], 16));
                    }

                }
            }
            return new String(buf, 0, j);
        }
        catch(Exception ex)
        {
            return null ;
        }

    }
    static char lowerCaseToUpperCase(char ch) {
        if (ch >= 97 && ch <= 122) { // 如果是小写字母就转化成大写字母
            ch = (char) (ch - 32);
        }
        return ch;
    }

    /**
     * 判断一个url是否需要编码，按需要增减过滤的字符
     *
     * @param url
     * @return
     */
    static boolean needEncoding(String url) {
        // 不需要编码的正则表达式
//      String allowChars = SystemConfig.getValue("ENCODING_ALLOW_REGEX",
//              Constants.ENCODING_ALLOW_REGEX);
        if (url.matches("^[0-9a-zA-Z.:/?=&%~`#()-+]+$")) {
            return false;
        }

        return true;
    }

}

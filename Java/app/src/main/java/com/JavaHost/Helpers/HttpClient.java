package com.JavaHost.Helpers;
import com.alibaba.fastjson2.JSON;
import com.alibaba.fastjson2.JSONObject;
import okhttp3.OkHttpClient;
import okhttp3.*;
import java.util.Map;
import java.util.concurrent.TimeUnit;

public class HttpClient {
    /**
     * Http 请求接口
     * @param _url
     * @param isPost 是否为Post 方法
     * @param timeOut 超时时间
     * @param headerjson headers json 字符串 形如：{"Content-Type", "application/json"}
     * @param contentString post 内容
     * @param contentMediaType  如："application/json; charset=utf-8"
     * @return 执行结果
     */
    public   String Request(String _url,boolean isPost,int timeOut,String headerjson,String contentString,String contentMediaType)
    {
        try
        {
             OkHttpClient client = new OkHttpClient.Builder()
                    .connectTimeout(timeOut, TimeUnit.SECONDS)
                    .writeTimeout(timeOut, TimeUnit.SECONDS)
                    .readTimeout(timeOut, TimeUnit.SECONDS)
                    .build();
             Request.Builder builder = new Request.Builder().url(_url) ;
             if(!isPost)
                builder.get() ;
             else
                 builder.post(RequestBody.create( MediaType.parse(contentMediaType), contentString));
             AddHeaders(builder,headerjson);
             Request request = builder.build();
            Response response = client.newCall(request).execute();
            if(response.isSuccessful())
            {
                return GetExceptionString(String.valueOf(response.code()),response.message(),response.body().string()) ;
            }
            else
             return response.body().string() ;
        }
        catch(Exception e)
        {
            e.printStackTrace();
            return GetExceptionString(e);
        }
    }

    String GetExceptionString(String... strArr)
    {
         int ncount = strArr.length ;
        StringBuilder errorMsg = new StringBuilder() ;
        errorMsg.append("error:") ;
        for (int i = 0; i < ncount; i++) {
            errorMsg.append(" "+strArr[i]);
        }
        return errorMsg.toString() ;
    }
    String GetExceptionString(Exception e)
    {
        e.printStackTrace();
        String errorMsg = String.format("error:%s %s ",e.getMessage(),e.getStackTrace()) ;
        return errorMsg ;
    }
      void AddHeaders( Request.Builder builder,String headerJson)
    {
        JSONObject jdata = JSON.parseObject(headerJson) ;
        for (Map.Entry<String, Object> entry: jdata.entrySet()) {
             String headKey =  entry.getKey();
             String value = entry.getValue().toString();
            builder.addHeader(headKey, value);
        }
    }
}

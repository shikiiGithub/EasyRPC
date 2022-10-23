package com.JavaHost.Helpers;

import System.*;
import System.Collections.Generic.Dictionary;
import com.auth0.jwt.JWT;
import com.auth0.jwt.JWTVerifier;
import com.auth0.jwt.algorithms.Algorithm;
import com.auth0.jwt.exceptions.*;
import com.auth0.jwt.interfaces.DecodedJWT;

import java.util.Date;
import java.util.HashMap;
import java.util.Map;

public class AAServer {
    String secret = "F7CC50FD71A64E14A54AC48CE44EC4DB";
    Algorithm algorithm;

    public AAServer()
    {
          algorithm = Algorithm.HMAC256(secret);
    }

    /// <summary>
    /// 生成Jwt Token
    /// </summary>
    /// <param name="name">user name</param>
    /// <param name="pwd">密码</param>
    /// <param name="expireTimeInHours">过期时间</param>
    /// <param name="deviceId">设备Id</param>
    /// <returns></returns>
  public  String GenJwtToken(String name,String pwd,int expireTimeInHours,String deviceId)
    {
        try
        {
            long expireGapMilsecs = expireTimeInHours* 60*60*1000;
            long nowMillis = System.currentTimeMillis();//生成JWT的时间
            Map<String, Object> payload = new HashMap<>();
            payload.put("Name", name);
            payload.put("Password", pwd);
            payload.put("DeviceId", deviceId);
            String token = JWT.create()
                    .withIssuedAt(new Date((nowMillis)))
                    .withExpiresAt(new Date(nowMillis+expireGapMilsecs))
                    .withPayload(payload)
                    .sign(algorithm);
            return token;
        }
        catch (Exception ex)
        {
            return this.GetErrorMessage(ex);
        }

    }
    /// <summary>
    /// 解析Jwt Token
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
  public   String ParseJwtToken(String token)
    {
        String json = null;
        try {

            JWTVerifier verifier = JWT.require(algorithm)
                    .build(); //Reusable verifier instance
            DecodedJWT jwt = verifier.verify(token);
            json = jwt.toString() ;
        }
        catch (AlgorithmMismatchException e ) //if the algorithm stated in the token's header is not equal to the one defined in the JWTVerifier.
        {
            json = "error:the algorithm stated in the token's header is not equal to the one defined in the JWTVerifier";
        }
        catch ( SignatureVerificationException e) //if the signature is invalid.
        {
            json = "error:Token has invalid signature";
        }
        catch (TokenExpiredException e) //if the token has expired.
        {
            json = "error:Token has expired";
        }
        catch ( InvalidClaimException e) //if a claim contained a different value than the expected one.
        {
            json = "error:a claim contained a different value than the expected one";
        }
        return json;
    }
    public String GetErrorMessage(String text)
    {
        return "error:" + text;
    }

    public String GetErrorMessage(Exception ex)
    {
        return "error:" + ex.getMessage() + " " + ex.getStackTrace();
    }
}

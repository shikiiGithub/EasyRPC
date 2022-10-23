 
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using LitJson;
using shikii.Hub.DI;
using shikii.Hub.Networking;

namespace shikii.Hub.WebApi
{
    public delegate bool InferUserAuthenticationActionCallback(LitJson.JsonData data, out String statusCode, out String desc);
    public delegate CTCMessage IdentifyJwtTokenCallback(CTCMessage message  );
    public delegate CTCMessage IdentifyUserCallback(CTCMessage message );
    public class RouteInfo
    {
        /// <summary>
        ///  控制器名如 HomeController 必须有Controller后缀
        /// </summary>
        public String ControllerName { get; set; }

        /// <summary>
        /// 控制器类里的方法名
        /// </summary>
        public String ActionName { get; set; }

        /// <summary>
        /// 控制器类时的方法反射信息
        /// </summary>
        public MethodInfo ThisMethod { get; set; }

        /// <summary>
        /// 控制器实例
        /// </summary>
        public Object ControllerInstance { get; set; }

        /// <summary>
        /// GET or POST
        /// </summary>
        public String Method { get; set; } = "GET";

        public ParameterInfo[] ParameterInfos { get; set; }

        /// <summary>
        /// URL 
        /// </summary>
        public String UrlPath { get { return String.Format("{0}{1}/{2}", Prefix, ControllerName, ActionName); } }

        /// <summary>
        /// 默认为 “/api/”
        /// </summary>
        public String Prefix { get; set; }  
    }
    public class NodeJsWebAPI
    {

        /// <summary>
        /// 路由信息
        /// </summary>
        public List<RouteInfo> Routes { get; set; }

        public NodeJsWebAPI()
        {
            this.Routes = new List<RouteInfo>();
        }


        public RouteInfo GetRouteInfo(String method,String controllerName, string methodName, String prefix="/api/")
        {
            RouteInfo routeInfo = new RouteInfo();
            routeInfo.Method = method;
            routeInfo.Prefix = prefix;
            routeInfo.ControllerName = controllerName;
            routeInfo.ActionName = methodName;
            this.Routes.Add(routeInfo);
            return routeInfo;   
        }

        public String  Execute(String path, LitJson.JsonData args, String method, String content, out string statusCode, out String desc)
        {
            statusCode = "200";
            desc = "";
            try
            {
                List<RouteInfo> items = this.Routes.FindAll(x =>
                {
                  
                    
                    bool urlPaired = path.ToLower().TrimEnd('/') == x.UrlPath.ToLower().TrimEnd('/');
                    bool methodPaired = x.Method.ToLower() == method.ToLower();
                    return urlPaired && methodPaired;
                }
            );
                if (items.Count == 0)
                {

                    desc = "url 有误未找到相应的Action";
                    statusCode = "404";
                    return "";
                }

                String resultText = "no returned data";
                if (items != null && items.Count > 0)
                {
                    RouteInfo thisItem = items[0];
                    List<Object> argObjs = null;
                    if (thisItem.Method.ToLower() == "get")
                    {

                        if (thisItem.ParameterInfos == null)
                        {
                            thisItem.ParameterInfos = thisItem.ThisMethod.GetParameters();

                        }
                        if (args.Count == 0 && thisItem.ParameterInfos.Length > 0)
                        {
                            desc = "provide args can't be zero !";
                            statusCode = "500";
                            return "";
                        }
                        if(args.Count == 0 && thisItem.ParameterInfos.Length==0)
                        {
                            goto invoke;
                        }
                        argObjs = new List<object>();
                        List<String> loweredPassedArgNames = args.Keys.Select(x => x.ToLower()).ToList();

                        foreach(ParameterInfo x in thisItem.ParameterInfos)
                        {

                            string paramName = x.Name.ToLower();
                            int index = loweredPassedArgNames.IndexOf(paramName);
                            if (index == -1)
                            {
                                desc = "provide args don't match the method args!";
                                statusCode = "500";
                                return "";

                            }
                            JsonData val = args[index];
                            if (x.ParameterType.BaseType == typeof(ValueType))
                            {
                                if (x.ParameterType == typeof(int))
                                {
                                    argObjs.Add((int)val);
                                }
                                else if (x.ParameterType == typeof(long))
                                {
                                    argObjs.Add((long)val);
                                }
                                else if (x.ParameterType == typeof(float))
                                {
                                    argObjs.Add((float)val);
                                }
                                else if (x.ParameterType == typeof(double))
                                {
                                    argObjs.Add((double)val);
                                }
                                else if (x.ParameterType == typeof(bool))
                                {
                                    argObjs.Add((bool)val);
                                }
                            }
                            else
                            {
                                if (x.ParameterType == typeof(String))
                                    argObjs.Add((string)val);
                                else
                                {
                                    string _json = val.ToJson();
                                    object ___obj = JsonMapper.ToObject(x.ParameterType, _json);
                                    argObjs.Add(___obj);
                                }

                            }
 
                        }
                           

                        }
                    else //处理Post 方法
                    {
                        if (!String.IsNullOrEmpty(content))
                        {

                            List<Type> argTypes = thisItem.ThisMethod.GetParameters().ToList().ConvertAll(x => x.ParameterType);
                            if (argTypes.Count > 0)
                            {
                                if (argTypes.Count == 1)
                                {
                                    Object obj = null;
                                    //todo: xujing 要做性能优化
                                    try
                                    {
                                        obj = JsonMapper.ToObject(argTypes[0], content);
                                    }
                                    catch
                                    {

                                        int firstToken = content.IndexOf('{');
                                        int lastToken = content.IndexOf('{');
                                        String realContent = content.Substring(firstToken + 1, lastToken - 1);
                                        obj = JsonMapper.ToObject(argTypes[0], content);
                                    }
                                    argObjs.Add(obj);
                                }
                                else
                                {
                                    JsonData jd = JsonMapper.ToObject(content);
                                    int nCount = argTypes.Count;
                                    List<String> keys = jd.Keys.ToList();
                                    for (int i = 0; i < nCount; i++)
                                    {

                                        JsonData jdata = jd[keys[i]];
                                        Type type = argTypes[i];
                                        if (type.BaseType == typeof(ValueType))
                                        {
                                            Type x = type;
                                            if (x == typeof(int))
                                            {
                                                argObjs.Add((int)jdata);
                                            }
                                            else if (x == typeof(long))
                                            {
                                                argObjs.Add((long)jdata);
                                            }
                                            else if (x == typeof(float))
                                            {
                                                argObjs.Add(jdata.GetSingle());
                                            }
                                            else if (x == typeof(double))
                                            {
                                                argObjs.Add(((double)jdata));
                                            }
                                            else if (x == typeof(bool))
                                            {
                                                argObjs.Add(((bool)jdata));
                                            }
                                        }
                                        else if (type == typeof(String))
                                        {
                                            String str = (String)jdata;
                                            argObjs.Add(str);
                                        }
                                        else
                                        {
                                            Object obj = JsonMapper.ToObject(type, jdata.ToJson());
                                            argObjs.Add(obj);
                                        }
                                    }



                                }

                            }
                        }
                    }

                invoke:;
                    Object result = null;
                    if (argObjs == null)
                    {
                        result = thisItem.ThisMethod.Invoke(thisItem.ControllerInstance, null);
                    }
                    else
                        result = thisItem.ThisMethod.Invoke(thisItem.ControllerInstance, argObjs.ToArray());
                    if (result is String)
                        resultText = (String)result;
                    else if (result.GetType().BaseType == typeof(ValueType))
                        resultText = result.ToString();
                    else
                        resultText = JsonMapper.ToJson(result);
                }
                return resultText;
            }
            catch (Exception ex)
            {
                statusCode = "500";
                desc = ex.Message + ex.StackTrace;
                return "failed:" + desc;
            }
        }

        public void SearchAndRegisterActions(DiManager ThisDi, Assembly asm)
        {

            Type[] types = asm.GetTypes();
            types.ToList().ForEach(type =>
            {
                if (type.Name.EndsWith("Controller") && type.Name != "BaseController")
                {
                    String controllerName = type.Name.Replace("Controller", "");

                    MethodInfo[] mifs = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                    if (mifs != null)
                        mifs.ToList().ForEach(method =>
                        {

                            String methodName = method.Name;

                            Attribute attr = method.GetCustomAttribute<GetAttribute>();
                            Object objController = ThisDi.GetService(type);

                            if (attr != null && attr is PostAttribute)
                            {
                                RouteInfo r = this.GetRouteInfo("POST", controllerName, methodName);
                                r.ThisMethod = method;
                                r.ControllerInstance = objController;
                            }

                            else
                            {
                                attr = method.GetCustomAttribute<PostAttribute>();
                                if (attr != null)
                                {
                                    RouteInfo r = this.GetRouteInfo("POST", controllerName, methodName);
                                    r.ThisMethod = method;
                                    r.ControllerInstance = objController;
                                }
                                else
                                {
                                    RouteInfo r = this.GetRouteInfo("GET", controllerName, methodName);
                                    r.ThisMethod = method;
                                    r.ControllerInstance = objController;
                                }

                            }

                        });

                }

            });

        }


        /// <summary>
        /// Nodejs 中继 http 请求给 .net 处理。
        /// 返回处理结果给 NodeJs再传给浏览器或客户端
        /// </summary>
        /// <param name="req">http 请求的 json 字符串</param>
        /// <returns>处理结果</returns>
        public String CommonWebAPIEntry(string req,bool enableIdCheck, IdentifyJwtTokenCallback identifyJwtToken, IdentifyUserCallback identifyUser, Func<Object, int> GetUserIdentifiedLevel)
        {
            Console.WriteLine("enter the NodeJs WebAPI Server Entry");
            string statusCode = "500", desc = "";
            LitJson.JsonData data = LitJson.JsonMapper.ToObject(req);
            String url = data["url"].ToString();
            String method = data["method"].ToString();
            String content = data["content"]?.ToString();
            String path = data["path"]?.ToString();
            LitJson.JsonData args = data["args"];
            bool auth = true;
            if(enableIdCheck)
            auth = InferUserAuthentication(data,identifyJwtToken,identifyUser,GetUserIdentifiedLevel, out statusCode, out desc);
            ObjectJsonResult _objectJsonResult = new ObjectJsonResult();
            Func<ObjectJsonResult, Object, String, bool, String, String> _getJsonResult = (objectJsonResult, data, msg, status, code) =>
            {
                objectJsonResult.Data = data;
                objectJsonResult.Msg = msg;
                if (status)
                {
                    objectJsonResult.Status = "success";
                    objectJsonResult.Code = float.Parse(code);
                }
                else
                {
                    objectJsonResult.Status = "error";
                    objectJsonResult.Code = float.Parse(code);
                }
                return LitJson.JsonMapper.ToJson(objectJsonResult);
            };
            if (auth)
            {

                String result = this.Execute(path,args, method, content, out statusCode, out desc);
                if (result != null && result.Trim().Length > 0)
                {
                    if (result.Trim()[0] != '{')
                    {
                        return _getJsonResult(_objectJsonResult, result, desc, statusCode == "200" ? true : false, statusCode);
                    }
                    else
                        return result;
                }
                else
                {
                    return _getJsonResult(_objectJsonResult, "", desc, false, statusCode);
                }
            }
            else
            {
                return _getJsonResult(_objectJsonResult, "", desc, false, statusCode);
            }
        }



        /// <summary>
        /// 验证用户是否登录，如果登录是否有访问Web Api 的权限
        /// </summary>
        /// <param name="statusCode">处理结果状态码</param>
        /// <param name="description">处理结果描述</param>
        /// <returns>用户是否登录</returns>
        bool InferUserAuthentication(LitJson.JsonData data, IdentifyJwtTokenCallback identifyJwtToken, IdentifyUserCallback identifyUser,Func<Object,int> GetUserIdentifiedLevel, out String statusCode, out String description)
        {

            /// <param name="url">htttp url</param>
            /// <param name="method">是"GET"还是“POST”</param>
            /// <param name="keyList">Header 的keys</param>
            /// <param name="Headers">http headers</param>
            String url = data["url"].ToString();
            String method = data["method"].ToString();
            String content = data["content"]?.ToString();
            LitJson.JsonData headers = data["headers"];
            Dictionary<String, String> Headers = new Dictionary<String, String>();
            List<String> keys = headers.Keys.ToList();
            for (int i = 0; i < headers.Count; i++)
            {
                Headers.Add(keys[i], headers[keys[i]].ToString());
            }

            List<String> keyList = keys;

            if (!keyList.Contains("Authentication"))
            {
                //todo 跳转到登录界面
                statusCode = "401.1";
                description = "无验证头！";
                return false;
            }
            else
            {
                string token = Headers["Authentication"];

                if (!String.IsNullOrEmpty(token))
                {
                    if (!String.IsNullOrEmpty(token.Trim()) && (token == "login" || token == "sign up"))
                    {
                        //放行

                        statusCode = "200";
                        description = "sucess";
                        return true;
                    }

                    else
                    {
                        //todo 跳转到登录界面
                        statusCode = "401.1";
                        description = "有验证头，但无验证值！";
                        return false;
                    }
                }

                if (String.IsNullOrEmpty(token))
                {
                    //todo 跳转到登录界面
                    statusCode = "401.1";
                    description = "有验证头，但无验证值！";
                    return false;

                }
                else
                {
                    token = token.Trim();
                    if (String.IsNullOrEmpty(token))
                    {
                        //todo 跳转到登录界面
                        statusCode = "401.1";
                        description = "有验证头，但无验证值！";
                        return false;
                    }
                    else
                    {

                        try
                        {
                            CTCMessage message = new CTCMessage();
                            message.Params = new object[] { token };
                            message = identifyJwtToken(message );
                            
                            if (message.ReturnedData.ToString().Contains("error"))
                            {
                                statusCode = "401.1";
                                description = message.ReturnedData.ToString();
                                return false;
                            }
                            else
                            {
                                message.Params = new object[] { message.ReturnedData };
                                message = identifyUser(message );
                              
                                if (message.ReturnedData != null)
                                {
                                    return CheckIdentityLevel(GetUserIdentifiedLevel(message.ReturnedData), url, method, out statusCode, out description);
                                }
                                else
                                {
                                    statusCode = "404";
                                    description = "未找到该用户";
                                    return false;
                                }

                            }
                        }
                        catch (Exception ex)
                        {
                            statusCode = "403";
                            description = "认证时出现异常！" + ex.Message + ex.StackTrace;
                            return false;
                        }



                    }

                }
            }
        }




        /// <summary>
        /// 验证访问Web Api 的权限等级
        /// </summary>
        /// <param name="user">用户信息</param>
        /// <param name="url">htttp url</param>
        /// <param name="method">是"GET"还是“POST”</param>
        /// <param name="statusCode">处理结果状态码</param>
        /// <param name="description">处理结果描述</param>
        /// <returns></returns>
        bool CheckIdentityLevel(int identityLevel, String url, String method, out string statusCode, out string description)
        {

            url = url.ToLower();
            List<RouteInfo> items = this.Routes.ToList().FindAll(x => url.Contains(x.UrlPath.ToLower()) && (x.Method.ToLower() == method.ToLower()));

            if (items.Count() == 0)
            {
                statusCode = "404";
                description = "未找到相应的Action";
                return false;
            }

            if (items.Count() > 1)
            {
                items.Sort((x, y) => x.UrlPath.Length - y.UrlPath.Length);
                items.RemoveRange(1, items.Count() - 1);

            }

            MethodInfo mif = items[0].ThisMethod;
            IdentityLevelAttribute attribute = mif.GetCustomAttribute<IdentityLevelAttribute>();
            if (attribute != null)
            {
                int idLevel = attribute.Level;
                if (identityLevel >= idLevel)
                {
                    statusCode = "200";
                    description = "sucess";
                    return true;
                }
                else
                {
                    statusCode = "401.4";
                    description = "该用户权限等级低";
                    return false;
                }
            }
            else
            {
                statusCode = "200";
                description = "sucess";
                return true;
            }


        }

      
        public static NodeJsWebAPI GetInstance(ServiceHost ThisHost,DiManager di,Assembly asm,object bookedServices)
        {
            NodeJsWebAPI webapiServer = new NodeJsWebAPI();
            webapiServer.SearchAndRegisterActions(di, asm);
            di.RegisterType<ServiceStatusManager>(DIClassAttribute.LifeCycleModes.Singleton);
            ServiceStatusManager ssm = di.GetService<ServiceStatusManager>();
            ssm.PrepareBookedServices(bookedServices);
            if (ThisHost != null)
            {
                ThisHost.RegisterSpyingService(ssm.ToString());
                ThisHost.OnSpyingServiceChanged += ssm.OnSpyingServiceChanged;
                ThisHost.OnDisconnect += () =>
                {
                    ssm.ResetSpyingServiceStatus();
                };

            }
            return webapiServer;
        }

        public bool ExistService(String serviceName)
        {
            DiManager diManager = shikii.Hub.Common.ExtensionFuncs.ThisDiManager as DiManager;
            ServiceStatusManager ssm = diManager.GetService<ServiceStatusManager>();
            return ssm.ServiceSatusDic[serviceName];
        }

    }
}

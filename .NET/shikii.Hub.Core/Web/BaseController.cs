using shikii.Hub.DI;
using shikii.Hub.Helpers;
using shikii.Hub.Networking;
using shikii.Hub.WebApi;
 
using System;
using System.Collections.Generic;
using System.Text;

namespace shikii.Hub.WebApi
{

    public abstract class BaseController
    {
        protected String appClassName = "App";
       
        public DiManager ThisDi { get; set; }
 
        public ServiceHost ThisHost { get; set; }
        public ServiceStatusManager ThisServiceSatusManager { get; set; }
        public BaseController()
        {
            ThisDi = (shikii.Hub.Common.ExtensionFuncs.ThisDiManager as DiManager);
  
        }



        /// <summary>
        /// 按以下内容实现就好
        /// if (this.ThisHost == null)
        ///    this.ThisHost = ThisDi.GetService<App>().ThisHost;
        /// </summary>
        /// <param name="diManager"></param>
        protected abstract void GetThisHost();

         CTCMessage CallServiceEx(String moduleName, String className, String MethodName, params object[] _params)
        {
           CTCMessage message  =  GetCTCMessageObject(moduleName, className, MethodName, _params);
            GetThisHost();
            message  =  this.ThisHost.CallService(moduleName, message,10*1000,10);
           return message; 
        }

         bool ExistService(String serviceName)
        {
            if(this.ThisServiceSatusManager == null)
                this.ThisServiceSatusManager = ThisDi.GetService<ServiceStatusManager>();
            return ThisServiceSatusManager.ServiceSatusDic[serviceName];    
        }

         CTCMessage GetCTCMessageObject(String moduleName,String className,String MethodName,params object[] _params)
        {
            CTCMessage message = new CTCMessage();
            message.AssemblyName = moduleName;
            message.ClassName = className;
            message.Params = _params;
            message.MethodName =MethodName;
            return message;

        }


        /// <summary>
        /// 用于成功则返回空（“”）否则有返回错误消息。
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="className">类名</param>
        /// <param name="methodName">方法名</param>
        /// <param name="customAction">自定义处理方式</param>
        /// <param name="_params"></param>
        /// <returns></returns>
        protected String ExecuteReturnStringAction(String service,string className,String methodName,Action<CTCMessage> customAction,params object[] _params)
        {
            try
            {
                if (!this.ExistService(service))
                {
                      String errorText =  this.GetErrorMessage(String.Format("未找到服务：{0} 或服务{0}已经停止！",service)) ;
                      return this.InternalGetErrorJsonResult(errorText,"404") ;
                }
                else
                {
                    CTCMessage message = CallServiceEx(service, className, methodName, _params);
                   
                    if(customAction != null)    
                      customAction(message);
                    if(!String.IsNullOrEmpty(message.ErrorMsg))
                    {
                        if (!String.IsNullOrEmpty(message.ErrorMsg.Trim()))
                        {
                            String errorText = GetErrorMessage(message.ErrorMsg);
                            return this.InternalGetErrorJsonResult(errorText);
                        }
                    }

                    if(message.ReturnedData !=null && !String.IsNullOrEmpty(message.ReturnedData.ToString().Trim()) && message.ReturnedData.ToString().Contains("error:"))
                    {
                        return this.InternalGetErrorJsonResult(message.ReturnedData.ToString());
                    }

                    String returnedDataString = message.ReturnedData.ToString();
                    return returnedDataString;
                }

            }
            catch (Exception ex)
            {

                string error =  GetErrorMessage(ex);

               return  this.InternalGetErrorJsonResult(error);
            }
        }

        /// <summary>
        /// 用于成功则返回结果对象否则有返回错误Json对象。
        /// </summary>
        /// <param name="service">服务名</param>
        /// <param name="className">类名</param>
        /// <param name="methodName">方法名</param>
        /// <param name="customAction">自定义处理方式</param>
        /// <param name="_params"></param>
        /// <returns></returns>
        protected String ExecuteReturnObjectAction(String service, string className, String methodName, Action<CTCMessage> customAction, params object[] _params)
        {
 
            try
            {
                if (!this.ExistService(service))
                {
                    String error = GetErrorMessage(String.Format("未找到服务：{0} 或服务{0}已经停止！", service));
                    return  this.InternalGetErrorJsonResult(  error, "404");

                }
                else
                {
                    CTCMessage message = CallServiceEx(service, className, methodName, _params);
                    if (customAction != null)
                        customAction(message);
                    if (!String.IsNullOrEmpty(message.ErrorMsg))
                    {
                        if (!String.IsNullOrEmpty(message.ErrorMsg.Trim()))
                        {
                            String error  =  GetErrorMessage(message.ErrorMsg);
                            return this.InternalGetErrorJsonResult( error);
                        }
                          
                    }

                    return this.InternalGetJsonResult( message.ReturnedData);
                }

            }
            catch (Exception ex)
            {
                String error = GetErrorMessage(ex);
                error = InternalGetErrorJsonResult(error);
                return error;
            }
        }

        protected String InternalGetErrorJsonResult(string msg ,string _code="500")
        {
            return InternalGetJsonResult("",msg, _code);
        }
        protected String InternalGetJsonResult( Object data, String msg="",  string _code="200")
        {
            ObjectJsonResult objectJsonResult = new ObjectJsonResult();
            objectJsonResult.Data = data;
            objectJsonResult.Msg = msg;
            if (_code == "200")
            {
                objectJsonResult.Status = "success";
                objectJsonResult.Code = float.Parse(_code);
            }
            else
            {
                objectJsonResult.Status = "error";
                objectJsonResult.Code = float.Parse(_code);
            }

            return GetJsonResult(objectJsonResult);

        }
        protected String GetErrorMessage(String text)
        {
            return "error:" + text;
        }
        protected String GetErrorMessage(Exception ex)
        {
            return String.Format("error: {0} {1}", ex.Message, ex.StackTrace) ;
        }

        protected String GetJsonResult(Object obj)
        {
             return LitJson.JsonMapper.ToJson(obj);
        }

        
    }
}

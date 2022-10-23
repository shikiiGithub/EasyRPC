using shikii.Hub.Networking;
using System;
using System.Collections.Generic;
using System.Text;

namespace shikii.Hub.WebApi
{
    public interface IWebApiApp
    {

        /// <summary>
        /// 检验是否JWT Token 通过
        /// </summary>
        /// <param name="message">传入的消息体</param>
        /// <returns>传出的消息体</returns>
        CTCMessage IdentifyJwtToken(CTCMessage message);
        /// <summary>
        /// 检验是否用户名与密码正确
        /// </summary>
        /// <param name="message">传入的消息体</param>
        /// <returns>传出的消息体</returns>

        CTCMessage IdentifyUser(CTCMessage message);
        /// <summary>
        /// 获得当前用户的权限等级
        /// </summary>
        /// <param name="_usr">用户实体</param>
        /// <returns>用户的权限等级</returns>

        int GetUserIdentifiedLevel(Object _usr);
        
    }
}

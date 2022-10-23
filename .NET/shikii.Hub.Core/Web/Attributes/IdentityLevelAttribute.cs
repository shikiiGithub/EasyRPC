using System;
using System.Collections.Generic;
using System.Text;

namespace shikii.Hub.WebApi
{
    public class IdentityLevelAttribute: Attribute
    {
        /// <summary>
        /// 用户授权等级
        /// </summary>
        public int Level { get; set; }
    }
}

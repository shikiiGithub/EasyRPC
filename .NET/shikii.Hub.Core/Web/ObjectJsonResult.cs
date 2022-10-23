using System;
using System.Collections.Generic;
using System.Text;

namespace shikii.Hub.WebApi
{
    public class ObjectJsonResult
    {
        /// <summary>
        /// Http 状态码
        /// </summary>
        public float Code { get; set; }
        /// <summary>
        /// 如果有错误发生则为错误内容
        /// </summary>
        public String Msg { get; set; }
        /// <summary>
        /// 当没有错误时结果内容
        /// </summary>
        public Object Data { get; set; }
        /// <summary>
        /// success or error
        /// </summary>
        public String Status { get; set; }    

    }
}

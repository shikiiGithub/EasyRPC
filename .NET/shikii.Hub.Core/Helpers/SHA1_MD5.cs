using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace shikii.Hub.Helpers
{
   public class SHA1_MD5
    {
        public static string ComputeFileSHA1(string FileName)
        {
            try
            {
                byte[] hr;
                using (SHA1Managed Hash = new SHA1Managed()) // 创建Hash算法对象
                {
                    using (FileStream fs = new FileStream(FileName, FileMode.Open))
                    // 创建文件流对象
                    {
                        hr = Hash.ComputeHash(fs); // 计算
                    }
                }
                return BitConverter.ToString(hr).Replace("-", ""); // 转化为十六进制字符串 
            }
            catch (IOException)
            {
                return "Error:访问文件时出现异常";
            }
        }
        //public static string ComputeFileMD5(string FileName)
        //{
        //    try
        //    {
        //        byte[] hr;
        //        using (MD5Cng Hash = new MD5Cng())
        //        {
        //            using (FileStream fs = new FileStream(FileName, FileMode.Open))
        //            // 创建文件流对象
        //            {
        //                hr = Hash.ComputeHash(fs); // 计算
        //            }
        //        }
        //        return BitConverter.ToString(hr).Replace("-", ""); // 转化为十六进制字符串 
        //    }
        //    catch (IOException)
        //    {
        //        return "Error:访问文件时出现异常";
        //    }
        //}

       // public static String ComputeStrin

    }
}

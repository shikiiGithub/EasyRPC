using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Linq;
using ICSharpCode.SharpZipLib.Zip;

namespace shikii.Hub.Common
{
   public class FileSystemManager
    {

      public static void GetFrameworkInfo(Assembly asm,out bool isCoreAsm  ,out String version )
        {
            isCoreAsm = false;
            version = "0";
            TargetFrameworkAttribute attribute = asm.GetCustomAttribute<TargetFrameworkAttribute>() ;
            if(attribute!= null)
            {
                if (attribute.FrameworkName.Contains("Framework")){
                    isCoreAsm = false;
                    version = attribute.FrameworkName.Replace(".NETFramework,Version=v", "");
                }
                else if(attribute.FrameworkName.Contains(".NETCoreApp"))
                {
                    isCoreAsm = true;
                    version = attribute.FrameworkName.Replace(".NETCoreApp,Version=v", "");
                }

            }
          
        }
      public static void RenameFile(String strSrc, string strDst)
        {
            File.Move(strSrc, strDst);
        }
      public static void BatchRenameFileByRemovePrefix(string FolderPath, string PrefixFileName, string extName = "*.*")
        {
            String[] FileNames = Directory.GetFiles(FolderPath, extName);

            foreach (var item in FileNames)
            {
                RenameFile(item, item.Replace(PrefixFileName, ""));
            }
        }

      public static void BatchRenameFileByUserDefineRule(string FolderPath, Func<String, String> UserDefineRule, string extName = "*.*")
        {
            String[] FileNames = Directory.GetFiles(FolderPath, extName);

            foreach (String item in FileNames)
            {
                RenameFile(item, UserDefineRule(item));
            }
        }
      public static void BatchRenameFileExtension(string FolderPath, string NewextName, string OldextName = "*.*")
        {
            String[] FileNames = Directory.GetFiles(FolderPath, OldextName);

            foreach (var item in FileNames)
            {

                RenameFile(item, Path.GetFileNameWithoutExtension(item) + NewextName);
            }
        }

        static void FindFiles(List<String> lst_FileNames, string strDirPath, String strSeekPattern, bool bRecordSubDir)
        {
            //在指定目录及子目录下查找文件,在list中列出子目录及文件
            try
            {
                DirectoryInfo Dir = new DirectoryInfo(strDirPath);
                DirectoryInfo[] DirSub = null;
                try
                {
                    DirSub = Dir.GetDirectories();
                }
                catch (Exception e)
                {

                    return;
                }

                if (DirSub.Length <= 0)
                { 
                    FileInfo[] fileInfoArr = Dir.GetFiles(strSeekPattern, SearchOption.TopDirectoryOnly);
                    foreach (FileInfo f in fileInfoArr) //查找文件
                    {
                        //listBox1.Items.Add(Dir+f.ToString()); //listBox1中填加文件名

                        lst_FileNames.Add(Dir + @"\" + f.ToString());
                    }
                }
                int t = 1;
                foreach (DirectoryInfo d in DirSub)//查找子目录 
                {
                    FindFiles(lst_FileNames,Dir + @"\" + d.ToString(), strSeekPattern, bRecordSubDir);
                    if (bRecordSubDir)
                        lst_FileNames.Add(Dir + @"\" + d.ToString());
                    if (t == 1)
                    {
                        FileInfo[] fileInfoArr = Dir.GetFiles(strSeekPattern, SearchOption.TopDirectoryOnly);
                        foreach (FileInfo f in fileInfoArr) //查找文件
                        {
                            lst_FileNames.Add(Dir + @"\" + f.ToString());
                        }
                        t = t + 1;
                    }
                }

            }
            catch (Exception ex)
            {


            }


        }
        public static void SeekFilesAndSubDirs(List<String> lst_FileNames,string strDirPath)
        {

            FindFiles(lst_FileNames,strDirPath, "*.*", true);
        }
        public static void SeekFiles(List<String> lst_FileNames,string strDirPath, string strSeekOption="*.*")
        {
            FindFiles(lst_FileNames,strDirPath, strSeekOption, false);
        }

        public static void CopyDirectory(string srcPath, string destPath)
        {
            try
            {
                DirectoryInfo dir = new DirectoryInfo(srcPath);
                FileSystemInfo[] fileinfo = dir.GetFileSystemInfos();  //获取目录下（不包含子目录）的文件和子目录
                foreach (FileSystemInfo i in fileinfo)
                {
                    if (i is DirectoryInfo)     //判断是否文件夹
                    {
                        if (!Directory.Exists(destPath + "\\" + i.Name))
                        {
                            Directory.CreateDirectory(destPath + "\\" + i.Name);   //目标目录下不存在此文件夹即创建子文件夹
                        }
                        CopyDirectory(i.FullName, destPath + "\\" + i.Name);    //递归调用复制子文件夹
                    }
                    else
                    {
                        File.Copy(i.FullName, destPath + "\\" + i.Name, true);      //不是文件夹即复制文件，true表示可以覆盖同名文件
                    }
                }
            }
            catch (Exception e)
            {
                throw;
            }
        }

        /// <summary>
        /// ZIP:解压一个zip文件
        /// add yuangang by 2016-06-13
        /// </summary>
        /// <param name="ZipFile">需要解压的Zip文件（绝对路径）</param>
        /// <param name="TargetDirectory">解压到的目录</param>
        /// <param name="Password">解压密码</param>
        /// <param name="OverWrite">是否覆盖已存在的文件</param>
        public static void UnZip(string ZipFile, string TargetDirectory, string Password, bool OverWrite = true)
        {
            //如果解压到的目录不存在，则报错
            if (!System.IO.Directory.Exists(TargetDirectory))
            {
                throw new System.IO.FileNotFoundException("指定的目录: " + TargetDirectory + " 不存在!");
            }
            //目录结尾
            if (!TargetDirectory.EndsWith("\\")) { TargetDirectory = TargetDirectory + "\\"; }

            using (ZipInputStream zipfiles = new ZipInputStream(File.OpenRead(ZipFile)))
            {
                zipfiles.Password = Password;
                ZipEntry theEntry;

                while ((theEntry = zipfiles.GetNextEntry()) != null)
                {
                    string directoryName = "";
                    string pathToZip = "";
                    pathToZip = theEntry.Name;

                    if (pathToZip != "")
                        directoryName = Path.GetDirectoryName(pathToZip) + "\\";

                    string fileName = Path.GetFileName(pathToZip);

                    Directory.CreateDirectory(TargetDirectory + directoryName);

                    if (fileName != "")
                    {
                        if ((File.Exists(TargetDirectory + directoryName + fileName) && OverWrite) || (!File.Exists(TargetDirectory + directoryName + fileName)))
                        {
                            using (FileStream streamWriter = File.Create(TargetDirectory + directoryName + fileName))
                            {
                                int size = 2048;
                                byte[] data = new byte[2048];
                                while (true)
                                {
                                    size = zipfiles.Read(data, 0, data.Length);

                                    if (size > 0)
                                        streamWriter.Write(data, 0, size);
                                    else
                                        break;
                                }
                                streamWriter.Close();
                            }
                        }
                    }
                }

                zipfiles.Close();
            }
        }

        public static void Unzip7Z(String _7zExePath,String zipFilePath,String outDirectoryPath)
        {

            if (File.Exists(zipFilePath))
            {
                Process p = new Process();
                p.StartInfo.FileName = _7zExePath;
                p.StartInfo.Arguments = " x  " + zipFilePath + " -o"+outDirectoryPath ;
                p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.Start();
                p.WaitForExit();
               
            }

        }

        public static Process SilentStart(String exePath,String args="")
        {
                Process p = new Process();
                 
                p.StartInfo.FileName = exePath;
                p.StartInfo.Arguments = " "+args;
                p.StartInfo.WorkingDirectory = Path.GetDirectoryName(exePath);
                p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.Start();
                return p;
        }


        public static Process Start(String exePath, String args = "")
        {
            Process p = new Process();
            p.StartInfo.FileName = exePath;
            p.StartInfo.Arguments = " " + args;
            p.StartInfo.WorkingDirectory = Path.GetDirectoryName(exePath);
            p.StartInfo.UseShellExecute = false;    //是否使用操作系统shell启动
            p.StartInfo.CreateNoWindow = false ;//不显示程序窗口
            p.Start();
            return p;
        }


        public static bool DeleteDir(string dirPath)
        {
            try
            {

                //去除文件夹和子文件的只读属性
                //去除文件夹的只读属性
                System.IO.DirectoryInfo fileInfo = new DirectoryInfo(dirPath);
                fileInfo.Attributes = FileAttributes.Normal & FileAttributes.Directory;

                //去除文件的只读属性
                System.IO.File.SetAttributes(dirPath, System.IO.FileAttributes.Normal);

                //判断文件夹是否还存在
                if (Directory.Exists(dirPath))
                {

                    foreach (string f in Directory.GetFileSystemEntries(dirPath))
                    {

                        if (File.Exists(f))
                        {
                            //如果有子文件删除文件
                            File.Delete(f);
                            Console.WriteLine(f);
                        }
                        else
                        {
                            //循环递归删除子文件夹
                            DeleteDir(f);
                        }

                    }

                    //删除空文件夹

                    Directory.Delete(dirPath);

                }
                return true;

            }
            catch (Exception ex) // 异常处理
            {
                return false;
            }

        }

       

    }
}

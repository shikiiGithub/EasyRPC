using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Timers;

namespace shikii.Hub.Helpers
{
    public class LogItemInfo
    {
        /// <summary>
        /// 服务名
        /// </summary>
        public String ServiceName { get; set; }
        /// <summary>
        /// 触发的日期
        /// </summary>
        public String FireDate { get; set; }    

        /// <summary>
        /// 触发时间（时分秒）
        /// </summary>
        public String FireTime { get; set; }

        /// <summary>
        /// 触发等级 DEBUG INFO  WARN ERROR
        /// </summary>
        public String  Level { get; set; }

        /// <summary>
        /// Id 可选
        /// </summary>
        public String Id { get; set; }  

        /// <summary>
        /// 日志内容
        /// </summary>
        public String Content { get; set; }

    }
    public class KiiLog : IDisposable
    {
        Queue<LogItemInfo> logItemInfos;
        String fileName;
        FileStream fileStream;
        TextWriter writer;
        int loopRecordLogTime = 500;
        int sizePerFile;
        String folderPath;
        DateTime currentDate;
        public KiiLog()
        {
            logItemInfos = new Queue<LogItemInfo>();
        }
       void RecordLogs()
        {
            while (true)
            {
                if (this.logItemInfos.Count > 0)
                {
                    cc:;
                    LogItemInfo item = this.logItemInfos.Dequeue();
                    String log = String.Format("[{0} {1}] <{2}> {3} {4} {5}", item.FireDate, item.FireTime, item.ServiceName, item.Level, item.Id, item.Content);
                    int gapDay = DateTime.Now.Day - currentDate.Day;
                     
                    if(gapDay == 0)
                    {
                        int nsize = (int)this.fileStream.Position;
                        if (nsize >= sizePerFile)
                        {
                            this.Dispose();
                            String[] arr = Path.GetFileNameWithoutExtension(this.fileName).Split('.', StringSplitOptions.RemoveEmptyEntries);
                            if (arr.Length == 1)
                            {
                                File.Move(this.fileName, Path.GetFileNameWithoutExtension(this.fileName) + ".0.txt");
                                this.fileName = Path.GetFileNameWithoutExtension(this.fileName) + ".1.txt";
                            }
                            else
                            {
                                String num = arr[1];
                                int _num = int.Parse(num);
                                _num++;
                                this.fileName = Path.GetFileNameWithoutExtension(this.fileName) + String.Format(".{0}.txt", _num);
                            }
                            fileStream = new FileStream(fileName, FileMode.CreateNew);
                            writer = new StreamWriter(fileStream);
                            writer.WriteLine(log);
                            

                        }
                        else
                        {
                            writer.WriteLine(log);
                           
                        }
                    }
                    else
                    {
                        currentDate = DateTime.Now;
                        GetFileName();
                        Dispose();
                        fileStream = new FileStream(fileName, FileMode.CreateNew);
                        writer = new StreamWriter(fileStream);
                        writer.WriteLine(log);
                        
                    }

                    if (logItemInfos.Count > 0)
                        goto cc;
                   writer.Flush();
                }

                Thread.Sleep(loopRecordLogTime);
            }
            
        }
       void GetFileName()
        {
            this.fileName = String.Format("{0}/log-{1}.txt", this.folderPath,DateTime.Now.ToString("yy-MM-dd"));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sizePerFile">日志文件每一个的大小（MB）</param>
        /// <param name="folderPath">日志文件所在的目录</param>
        /// <param name="loopRecordLogTime">每隔多少秒记录日志到文件</param>
        public void Prepare(int sizePerFile=2, String folderPath="logs", int loopRecordLogTime=1000)
        {
            this.folderPath = folderPath;   
            this.sizePerFile = sizePerFile*1024*1024;
            GetFileName();
            if(!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);  
            }
            if (!File.Exists(fileName))
                fileStream = new FileStream(fileName, FileMode.CreateNew);
            else
                fileStream = new FileStream(fileName, FileMode.Append);
            writer = new StreamWriter(fileStream);
            this.currentDate = DateTime.Now;
            Thread thd = new Thread(RecordLogs);
            thd.Start();
        }

        public void InternalLog (LogItemInfo item)
        {
            String log = String.Format("[{0} {1}] {2} {3} {4} {5}", item.FireDate, item.FireTime, item.ServiceName, item.Level, item.Id, item.Content);
            writer.WriteLine(log);
            writer.Flush();
        }
        public void Log(LogItemInfo item)
        {
             
            logItemInfos.Enqueue(item);
            String log = String.Format("[{0} {1}] {2} {3} {4} {5}",item.FireDate,item.FireTime,item.ServiceName,item.Level,item.Id,item.Content);
            Console.WriteLine(log);
        }

        public void Dispose()
        {
            try
            {
                this.writer.Close();
                this.writer.Dispose();
                this.fileStream.Close();
                this.fileStream.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " " + ex.StackTrace);
                
            }
        
        }
        ~KiiLog()
        {
            Dispose();  
        }
    }
}

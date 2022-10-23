using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
namespace shikii.Hub.Networking 
{
    public class RequestResultInfo
    {
        public String SourceServiceName { get; set; }
   
        public byte[] RawData { get; set; }

        public byte[] RawSendingContent{get;set;}
        public Encoding TextEncode { get; set; }

        public byte [] TaskIdBuffer { get; set; }  
  

 

        public byte[] GetRequestBuffer()
        {
             
                byte[] buf = this.RawData;
                byte sourceServiceNameLen = buf[TCPBase.MARKPOSITION];
                String sourceServiceName = this.TextEncode.GetString(buf, TCPBase.MARKPOSITION + 1, sourceServiceNameLen);
                this.SourceServiceName = sourceServiceName;
                byte[] msgBuf = buf.Skip(TCPBase.MARKPOSITION + sourceServiceNameLen + 1).ToArray();
                TaskIdBuffer = msgBuf.Skip(msgBuf.Count() - 8).ToArray();
                byte[] realbuf = msgBuf.SkipLast(8).ToArray();
                return realbuf;
         
          
          
        }
        public byte[] GetSendingContent()
        {
            
                byte[] result = null;
                result = new byte[RawSendingContent.Length + 8];
                RawSendingContent.CopyTo(result, 0);
                this.TaskIdBuffer.CopyTo(result, result.Length - 8);
                byte[] sourceServiceNameByts = this.TextEncode.GetBytes(SourceServiceName);
                byte sourceServiceNameLen = (byte)sourceServiceNameByts.Length;
                int bufLen = TCPBase.MARKPOSITION + sourceServiceNameByts.Length + 1 + result.Length;
                byte[] newBuffer = new byte[bufLen];
                newBuffer[TCPBase.MARKPOSITION] = sourceServiceNameLen;
                sourceServiceNameByts.CopyTo(newBuffer, TCPBase.MARKPOSITION + 1);
                TCPBase.StoreMSGMark(newBuffer, Signals.BYTES_CTC_NoLoop);
                TCPBase.StoreDataLenByts((uint)bufLen, newBuffer);
                result.CopyTo(newBuffer, TCPBase.MARKPOSITION + sourceServiceNameLen + 1);
                return newBuffer;
            
          
        }

        public byte[] GetErrorBytes(Exception ex)
        {
            String  statusMessage = ex.Message + " " + ex.StackTrace;
            byte [] errorBuf = this.TextEncode.GetBytes(statusMessage);
            byte[] result  = new byte[errorBuf.Length + 8];
            errorBuf.CopyTo(result, 0);
            this.TaskIdBuffer.CopyTo(result, result.Length - 8);
            byte[] sourceServiceNameByts = this.TextEncode.GetBytes(SourceServiceName);
            byte sourceServiceNameLen = (byte)sourceServiceNameByts.Length;
            int bufLen = TCPBase.MARKPOSITION + sourceServiceNameByts.Length + 1 + result.Length;
            byte[] newBuffer = new byte[bufLen];
            newBuffer[TCPBase.MARKPOSITION] = sourceServiceNameLen;
            sourceServiceNameByts.CopyTo(newBuffer, TCPBase.MARKPOSITION + 1);
            TCPBase.StoreMSGMark(newBuffer, Signals.BYTES_CTC_NoLoop);
            TCPBase.StoreDataLenByts((uint)bufLen, newBuffer);
            result.CopyTo(newBuffer, TCPBase.MARKPOSITION + sourceServiceNameLen + 1);
            return newBuffer;
        }

       
    }
}

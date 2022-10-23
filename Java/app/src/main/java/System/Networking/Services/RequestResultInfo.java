package System.Networking.Services;

import System.BitConverter;
import System.Text.Encoding;
import System.Networking.Signals;
import System.Networking.TCPBase;

public class RequestResultInfo {
    public String SourceServiceName  ;
    public byte[] RawData  ;
    public byte[] RawSendingContent ;
    public Encoding TextEncode  ;
    public byte [] TaskIdBuffer  ;
    byte[] Skip(byte[] src, int start) {
        int len = src.length - start;
        byte[] bytes = new byte[len];
        for (int i = start; i < src.length; i++) {
            bytes[i - start] = src[i];
        }
        return bytes;
    }

    byte[] SkipLast(byte[] src, int len) {
        byte[] bytes = new byte[src.length - len];

        for (int i = 0; i < bytes.length; i++) {
            bytes[i] = src[i];
        }
        return bytes;
    }

    public byte[] GetRequestBuffer() {

        byte [] buf = this.RawData ;
        byte sourceServiceNameLen = buf[TCPBase.MARKPOSITION];
        String sourceServiceName = this.TextEncode.GetString (buf, TCPBase.MARKPOSITION + 1, sourceServiceNameLen);
        this.SourceServiceName = sourceServiceName;
        byte[] msgBuf = Skip(buf, TCPBase.MARKPOSITION + sourceServiceNameLen + 1);
        byte[] timeTicks = Skip(msgBuf, msgBuf.length - 8);
        this.TaskIdBuffer = timeTicks;
        byte[] realbuf = SkipLast(msgBuf, 8);
        return  realbuf  ;
    }

    public byte[] GetSendingContent()
    {
        byte[] sourceServiceNameByts = this.TextEncode.GetBytes(this.SourceServiceName);
        int bufLen = TCPBase.MARKPOSITION + sourceServiceNameByts.length + 1 + this.RawSendingContent.length+8;
        byte [] result = new byte[bufLen];
        byte[] timeTicks = this.TaskIdBuffer;
        TCPBase.StoreMSGMark(result, Signals.BYTES_CTC_NoLoop);
        TCPBase.StoreDataLenByts(bufLen, result);
        result[TCPBase.MARKPOSITION] = (byte)sourceServiceNameByts.length;
        BitConverter.CopyTo(sourceServiceNameByts, result, TCPBase.MARKPOSITION + 1);
        BitConverter.CopyTo(this.RawSendingContent, result, TCPBase.MARKPOSITION + sourceServiceNameByts.length + 1);
        BitConverter.CopyTo(timeTicks, result, result.length - 8);
        return result;
    }

    public byte[] GetErrorBytes(Exception e)
    {
        String errorText = e.getMessage() + " " + e.getStackTrace();
        byte [] tempByts = this.TextEncode.GetBytes(errorText);
        byte [] timeTicks = this.TaskIdBuffer;
        byte [] result = new byte[tempByts.length + 8];
        byte[] sourceServiceNameByts = this.TextEncode.GetBytes(this.SourceServiceName);
        BitConverter.CopyTo(tempByts, result, 0);
        BitConverter.CopyTo(timeTicks, result, result.length - 8);
        int bufLen = TCPBase.MARKPOSITION + sourceServiceNameByts.length + 1 + result.length;
        byte[] newBuffer = new byte[bufLen];
        newBuffer[TCPBase.MARKPOSITION] = (byte)sourceServiceNameByts.length;
        BitConverter.CopyTo(sourceServiceNameByts, newBuffer, TCPBase.MARKPOSITION + 1);
        TCPBase.StoreMSGMark(newBuffer, Signals.BYTES_CTC_NoLoop);
        TCPBase.StoreDataLenByts(bufLen, newBuffer);
        BitConverter.CopyTo(result, newBuffer, TCPBase.MARKPOSITION + sourceServiceNameByts.length + 1);
        return result;
    }

}

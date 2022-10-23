package System.Networking;
import java.io.IOException;
import java.io.InputStream;
import java.net.InetSocketAddress;
import java.net.Socket;
import java.net.SocketAddress;
import java.util.function.Predicate;
import System.*;
public class TCPClient extends TCPBase {
    protected Socket Client;
    public Predicate<byte[]> Route = null  ;
    public   Predicate<TCPClient>  OnServerExit= null ;
    public boolean bIsConnected = false;
    protected SocketAddress serverAddress ;
    public TCPClient(String strIP, int nPort) {
        this.setStrIP(strIP);
        this.nPort = nPort;
    }
    public boolean Connect(int nTimeout) {
        try {
            Client = new Socket();
            Client.setReceiveBufferSize(this.BufferSize);
            Client.setSendBufferSize(this.BufferSize);
            this.serverAddress = new InetSocketAddress(getStrIP(), nPort);
            Client.connect(this.serverAddress, nTimeout);
            thd_Main = new Thread(()->{this.Loop();} );
            thd_Main.start();
            this.bIsConnected = true ;
            return true;
        } catch (IOException e) {
            this.bIsConnected = false ;
            System.out.println("at Connect :"+e.getMessage());

            return false;
        }
    }

    public boolean Reconnect(int timeOut)
    {
        try {

            Client = new Socket();
            Client.setReceiveBufferSize(this.BufferSize);
            Client.setSendBufferSize(this.BufferSize);
            this.serverAddress = new InetSocketAddress(getStrIP(), nPort);
            Client.connect(this.serverAddress, timeOut);
            thd_Main = new Thread(()->{this.Loop();} );
            thd_Main.start();
            this.bIsConnected = true ;
            return true;
        } catch (IOException e) {
            this.bIsConnected = false ;

            System.out.println( "at reconnect:"+ e.getMessage() + e.getStackTrace());
            return false;
        }
    }

    void ClearDataPipe()
    {

    }

    @Override
    protected void Loop() {
            this.bEndNetwork = false;
            while (true) {
                if(this.bEndNetwork)
                    break;
                InternalHandleSocketMessage();
            }

    }

    protected  void InternalHandleSocketMessage()
    {
        try
        {

            byte[] headBuf = new byte[5];
            InputStream   in = Client.getInputStream();
            in.read(headBuf, 0, TCPBase.MARKPOSITION);
            int nTotalLen = BitConverter.ToInt(headBuf, 1);
            byte[] buffer = new byte[nTotalLen];
            int nCount = TCPBase.MARKPOSITION;
            BitConverter.CopyTo(headBuf, buffer, 0);
            while (true) {
                if (nCount < nTotalLen) {
                    nCount += in.read(buffer, nCount, nTotalLen - nCount);
                } else
                    break;
            }
            byte byt_MSG_Mark = buffer[0];
            if (Route != null)
                Route.test(buffer);
        }
        catch (Exception e)
        {
            this.bIsConnected = false ;
            this.bEndNetwork = true;
            e.printStackTrace();
            if(this.OnServerExit != null) {
                if(Client != null)
                    try
                    {
                        Client.close();

                    }
                    catch(Exception ex)
                    {
                        ex.printStackTrace();
                    }

                this.OnServerExit.test(this);
            }
        }
    }

    @Override
    protected boolean Close() {
        this.bEndNetwork = true ;
        return false;
    }
}
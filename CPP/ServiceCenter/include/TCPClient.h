#ifndef TCPCLIENT_H
#define TCPCLIENT_H
#include "TCPBase.h"

namespace shikii
{
    namespace Hub
    {
        namespace Networking
        {
			#ifdef _WIN32
            DWORD WINAPI MessageThreadLoop(LPVOID p);
			#elif __linux
		      void MessageThreadLoop(void* p);
			#endif 
            class TCPClient : public TCPBase
            {

            protected:
                ~TCPClient()
                {
					#ifdef _WIN32
                    closesocket(thisSocket);
					#elif __linux
					 close(thisSocket);
					#endif
                }
                virtual void Route(byte msgKind, byte *buffer, int messageLen)
				{
				}
				virtual void  ConnectedCallback(const char *szClientId)
				{
				}
				virtual void   DisconnectedCallback(const char *szClientId)
				{
				}
            private:
            public:
            string clientId;
             SOCKET thisSocket;
             TCPClient()
                {
                    //创建socket
                    this->thisSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
					#ifdef _WIN32
                    if (INVALID_SOCKET == thisSocket)
                    {
                        cout<<"\r\nSocket 初始化失败"<<endl;
                        return;
                    }
					#endif
                }
                bool Connect(string ip,int port ,int bufferSize = 8688)
                {
                    this->Prepare(ip,port) ;
                    this->SystemBufferSize = bufferSize;
                    SetSystemBufferSize(thisSocket);
                   return Reconnect(ip,  port ,  bufferSize) ;
                }

				bool Reconnect(string ip,int port ,int bufferSize = 8688)
				{
					  //向服务器发出连接请求，当然我们也可以通过connet函数的返回值判断到底有无连接成功。
                    int iRetVal = connect(thisSocket, ( sockaddr *)&sockAddr, sizeof(sockAddr));
				#ifdef _WIN32
                    if (SOCKET_ERROR == iRetVal)
                    {
                        printf("\r\n连接服务器失败！\r\n");
                        closesocket(thisSocket);
                        return false;
                    }
				#endif
						clientId = inet_ntoa(sockAddr.sin_addr);
						clientId.append(":");
					    stringstream ss;
					    ss << sockAddr.sin_port;
					    clientId.append(ss.str());
				#ifdef _WIN32
                        HANDLE hThread;
					    DWORD threadId;
					    hThread = CreateThread(NULL, 0, MessageThreadLoop, this, CREATE_SUSPENDED, &threadId);
					    ResumeThread(hThread);
						#elif __linux__
						  std::thread thd(MessageThreadLoop) ;
						#endif
                        printf("\r\n连接服务器成功！\r\n");
                        return true ;
				}
                void MainLoop()
				{
					SOCKET client = this->thisSocket ;
					char *lenbuffer = new char[MARKPOSITION];
					while (1)
					{
					receiveStat:;
						int receivedBytes = recv(client, lenbuffer, MARKPOSITION, 0);
						// get message len
						int messageLen = this->FetchDataLenByts((byte *)lenbuffer);
						byte messageKind = lenbuffer[0];
						if (messageLen == 0)
							goto receiveStat;
						int nLen = messageLen;
						byte *buffer = new byte[nLen - MARKPOSITION];
						int nCount = MARKPOSITION;
						int nTotalLen = (int)nLen;
						//然后循环读取，确保没有少读
						while (true)
						{
							if (nCount < nTotalLen)
							{
								int r = recv(client, (char *)buffer + (nCount - MARKPOSITION), nTotalLen - nCount, 0);
								if (r == -1)
								{
									this->DisconnectedCallback(clientId.c_str());
									#ifdef _WIN32
									    closesocket( thisSocket) ;
									#elif __linux
										close( thisSocket) ;
									#endif
									delete[] buffer;
									goto finished;
								}
								else
								{
									nCount += r;
								}
							}
							else
								break;
						}
						Route(messageKind,buffer, nLen - MARKPOSITION);
						delete[] buffer;
					}
				finished:;
				}
                void Send(  byte msg, string &content)
				{
					SOCKET & client = this->thisSocket;
					int contentLen = content.size();
					byte *buf = new byte[MARKPOSITION + contentLen];
					byte *lenByts = GetBytes(contentLen + MARKPOSITION);
					memcpy(buf + 1, lenByts, 4);
					buf[0] = msg;
					memcpy(buf + MARKPOSITION, content.c_str(), content.size());
					send(client, (char *)buf, MARKPOSITION + contentLen, 0);
					delete[] lenByts;
				}

            };
			#ifdef _WIN32
	       DWORD WINAPI MessageThreadLoop(LPVOID p)
			{
				TCPClient *thisServer = (TCPClient *)p;
				thisServer->MainLoop();
				return 0;
			}
#elif __linux__
            void   MessageThreadLoop(void* p)
			{
				TCPClient *thisServer = (TCPClient *)p;
				thisServer->MainLoop();
				 
			}
 #endif
        } // namespace Networking
    }     // namespace Hub
} // namespace shikii

#endif
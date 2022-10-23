#ifndef TCPSERVER_H
#define TCPSERVER_H

#include "TCPBase.h"
namespace shikii
{
	namespace Hub
	{
		namespace Networking
		{	
			#ifdef _WIN32
			DWORD WINAPI Loop(LPVOID p);
			DWORD WINAPI ClientLoop(LPVOID p);
			#elif __linux__
			void  Loop(void* p);
			 void  ClientLoop(void* p);
			#endif
		
			class TCPServer : public TCPBase
			{
			public:
				int ListenBackLogCount;
				vector<SOCKET> Clients;
				vector<string> ClientIDs;
				#ifdef _WIN32
				vector<DWORD> ThreadIds;
				#elif __linux__
				vector<std::thread::id> ThreadIds;
				vector <std::thread*> Threads ; 
				#endif
				TCPServer()
				{
					ListenBackLogCount = 10;
				}
 
				bool Boot(string ip, int port, int bufferSize = 8688)
				{
					try
					{
						this->SystemBufferSize = bufferSize;
						serverSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP); 
						
						#ifdef _WIN32
						if (serverSocket == INVALID_SOCKET)
						{
							cout << "Socket error" << endl;
							return false;
						}
						#elif __linux__
						  bzero(&sockAddr, sizeof(sockAddr));	
						#endif
						this->Prepare(ip, port);
						 #ifdef _WIN32
						 if (bind(serverSocket, (sockaddr *)&sockAddr, sizeof(sockAddr)) == SOCKET_ERROR)
						{
							cout << "Bind error" << endl;
							return false;
						}
						if (listen(serverSocket, this->ListenBackLogCount) == SOCKET_ERROR)
						{
							cout << "Listen error" << endl;
							return false;
						}
						 #elif __linux__
						 bind(serverSocket, (sockaddr *)&sockAddr, sizeof(sockAddr)) ;
						 listen(serverSocket, this->ListenBackLogCount) ;
						 #endif
						
						SetSystemBufferSize(serverSocket);
						 #ifdef _WIN32
						HANDLE hThread;
						DWORD threadId;
						hThread = CreateThread(NULL, 0, Loop, this, CREATE_SUSPENDED, &threadId);
						ResumeThread(hThread);
						#elif __linux__
                               //定义 线程变量 定义的时候线程就会开始执行
							   Threads.push_back( new thread (Loop,this)) ;
						#endif
						return true;
					}
					catch (exception *e)
					{

						cout << e->what() << endl;
						return false;
					}
				}
				void MainLoop()
				{
					while (!this->bEndNetwork)
					{
						SOCKET clientSocket;
					 
						sockaddr_in client_sin;
						#ifdef _WIN32
						int len = sizeof(client_sin);
						#elif __linux__
						unsigned int len = sizeof(client_sin);
						#endif
						clientSocket = accept(this->serverSocket, (sockaddr *)&client_sin, &len);
						SetSystemBufferSize(clientSocket);
						this->Clients.push_back(clientSocket);
						string clientId;
						clientId = inet_ntoa(client_sin.sin_addr);
						clientId.append(":");
						stringstream ss;
						ss << client_sin.sin_port;
						clientId.append(ss.str());
						this->ClientIDs.push_back(clientId);
						this->ClientConnectedCallback(clientId.c_str());
						 
						#ifdef _WIN32
						HANDLE hThread;
						DWORD threadId;
						hThread = CreateThread(NULL, 0, ClientLoop, this, CREATE_SUSPENDED, &threadId);
						ResumeThread(hThread);
						#elif __linux__
						 
						 Threads.push_back(new thread(ClientLoop,this)) ;
						 thread::id threadId =  Threads[Threads.size()-1]->get_id() ;
						#endif
						ThreadIds.push_back(threadId);

					}
				}
				#ifdef _WIN32
				void SubLoop(DWORD threadId)
				{
					vector<DWORD>::iterator itor = find(this->ThreadIds.begin(), this->ThreadIds.end(), threadId);
				#elif __linux__
				void SubLoop(std::thread::id threadId)
				{
				     vector<thread::id>::iterator itor = find(this->ThreadIds.begin(), this->ThreadIds.end(), threadId);
				#endif
					int index = distance(this->ThreadIds.begin(), itor);
					SOCKET client = Clients[index];
					char *lenbuffer = new char[MARKPOSITION];
					while (true)
					{
					receiveStat:;
						int receivedBytes = recv(client, lenbuffer, MARKPOSITION, 0);
						// get message len
						int messageLen = this->FetchDataLenByts((byte *)lenbuffer);
						byte messageKind = lenbuffer[0];
						if (messageLen == 0)
							goto receiveStat;
						int nLen = messageLen;
						 
						byte *buffer = NULL ;
						try	{
                          buffer =  new byte[nLen - MARKPOSITION];
						}
						catch(exception e)
						{
							goto receiveStat;
						}
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
									this->ClientDisconnectedCallback(ClientIDs[index].c_str());
									if(index < this->ClientIDs.size())
									this->ClientIDs.erase(this->ClientIDs.begin() + index);
									if(index < this->ThreadIds.size())
									this->ThreadIds.erase(this->ThreadIds.begin() + index);
									#ifdef _WIN32
									closesocket( Clients[index]) ;
									#elif __linux
										close( Clients[index]) ;
									#endif
									if(index<this->Clients.size())
									this->Clients.erase(this->Clients.begin() + index);
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
						Route(messageKind, this->ClientIDs[index].c_str(), buffer, nLen - MARKPOSITION);
						delete[] buffer;
					}
				finished:;
				}

			protected:
				SOCKET serverSocket;
				virtual void Route(byte msgKind, const char *szClientId, byte *buffer, int messageLen)
				{
				}
				virtual void ClientConnectedCallback(const char *szClientId)
				{
				}
				virtual void ClientDisconnectedCallback(const char *szClientId)
				{
				}

				SOCKET GetClientSocket(string clientId)
				{
					vector<string>::iterator it = std::find(ClientIDs.begin(), ClientIDs.end(), clientId);
					int index = distance(ClientIDs.begin(), it);
					SOCKET client = this->Clients[index];
					return client;
				}

				void Send(string clientId, byte msg, string content)
				{

					SOCKET client = this->GetClientSocket(clientId);
					int contentLen = content.size();
					byte *buf = new byte[MARKPOSITION + contentLen];
					byte *lenByts = GetBytes(contentLen + MARKPOSITION);
					memcpy(buf + 1, lenByts, 4);
					buf[0] = msg;
					memcpy(buf + MARKPOSITION, content.c_str(), content.size());
					send(client, (char *)buf, MARKPOSITION + contentLen, 0);
					delete[] lenByts;
				}
          

           ~TCPServer()
		   {
			    vector<SOCKET>::iterator it = this->Clients.begin() ;
                while ( it != this->Clients.end())
				{
					 SOCKET  sct = *it ;
					 #ifdef _WIN32
                     closesocket(sct) ;
					 #elif __linux__
					 close(sct) ;
					 #endif
					 it++ ;
				}
				 #ifdef _WIN32
				closesocket(serverSocket) ;
				 #elif __linux__
				 close(serverSocket) ;
				 #endif
			    ClientIDs.clear() ;
			    ThreadIds.clear();
		   }

			};
#ifdef _WIN32
			DWORD WINAPI Loop(LPVOID p)
			{
				TCPServer *thisServer = (TCPServer *)p;
				DWORD threadId = GetCurrentThreadId();
				thisServer->MainLoop();
				return 0;
			}

			DWORD WINAPI ClientLoop(LPVOID p)
			{
				TCPServer *thisServer = (TCPServer *)p;
				DWORD threadId = GetCurrentThreadId();

				thisServer->SubLoop(threadId);
				return 0;
			}
#elif __linux__
			void  Loop(void* p)
			{
				TCPServer *thisServer = (TCPServer *)p;
				thisServer->MainLoop();
		 
			}

			void  ClientLoop(void * p)
			{
				TCPServer *thisServer = (TCPServer *)p;
				std::thread::id threadId = std::this_thread::get_id();

				thisServer->SubLoop(threadId);
			 
			}
			#endif
		}
	}
}
#endif
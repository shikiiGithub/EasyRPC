#ifndef SERVICEHOST_H
#define SERVICEHOST_H
#include "TCPClient.h"

#include <queue>
namespace shikii
{
	namespace Hub
	{
		namespace Networking
		{
			#ifdef _WIN32
			DWORD WINAPI ExecuteTaskQueueThread(LPVOID p) ;
	         DWORD WINAPI SendingQueueThread(LPVOID p) ;
			 #elif __linux__
			 void  ExecuteTaskQueueThread(void* p) ;
	         void  SendingQueueThread(void* p) ;
			 #endif
			class ServiceHost : public TCPClient
			{
			public:
				map<long, byte *> handledResultBufferDic;
				// int DaemonThreadNum = 3;
				// vector<DaemonThreadInfo> DaemonThreads;
				queue<byte*> SendingQueue;
				queue<RequestInfo> RequestInfoQueue ;
				ServiceHost()
				{
#ifdef _WIN32
					HANDLE hThread,hThreadExe;
					DWORD threadId,threadExe;
					hThread = CreateThread(NULL, 0, SendingQueueThread, this, CREATE_SUSPENDED, &threadId);
					hThreadExe = CreateThread(NULL, 0, ExecuteTaskQueueThread, this, CREATE_SUSPENDED, &threadExe);
					ResumeThread(hThread);

					printf("\r\n创建 SendingQueue 线程成功！\r\n");
#elif __linux__
                 thread thd(SendingQueueThread) ;
				  thread thd2(ExecuteTaskQueueThread);

#endif
				}
				string serviceName = "";
				bool RegisterService(string serviceName)
				{
					try
					{
#ifdef _WIN32
						int pid = GetCurrentProcessId();
#elif __linux__
						int pid = -1; // TODO linux 获得pid
#endif
						stringstream ss;
						ss << "{\"Name\": \"" << serviceName.c_str() << "\", \"ProcId\":" << pid << "}";
						string json = ss.str();
						this->Send(shikii::Hub::Networking::Signals::REGISTER_SERVICE, json);
						return true;
					}
					catch (exception e)
					{
						cout << e.what() << endl;
						return false;
					}
				}
				void SendingQueueLoop()
				{
					while (1)
					{
						while (!SendingQueue.empty())
						{
							byte *currentBuf = SendingQueue.front();
							int len = this->FetchDataLenByts(currentBuf);
							send(this->thisSocket, (char *)currentBuf, len, 0);
							 SendingQueue.pop();
							delete[] currentBuf;
						}
						Sleep(50);
					}
				}
				void ExecuteTaskQueueLoop()
				{
					while (1)
					{
						while (!RequestInfoQueue.empty())
						{
							RequestInfo & current = RequestInfoQueue.front();
                            string& strResult =  current.Invoke() ;
							RequestInfoQueue.pop();
						}
						Sleep(50);
					}
				}

			protected:
				virtual void Route(byte msgKind, byte *buffer, int messageLen)
				{
					switch (msgKind)
					{
					case shikii::Hub::Networking::Signals::BYTES_CTC:
						InternalHandleCTCMessage(buffer, messageLen);
						break;
					case shikii::Hub::Networking::Signals::BYTES_CTC_NoLoop:
						InternalHandleCTCMessageNoLoop(buffer, messageLen);
						break;
					case shikii::Hub::Networking::Signals::SpyingServiceChanged:
						SpyingServiceChanged(buffer, messageLen);
						break;
					}
				}
				virtual void ConnectedCallback(const char *szClientId)
				{
					cout << "ServiceHost is connected to server !" << endl;
				}
				virtual void DisconnectedCallback(const char *szClientId)
				{
					cout << "ServiceHost " << szClientId << " is offline" << endl;
					bool isConnected = false;
				reconnect:;
					isConnected = Reconnect(this->Ip, this->Port, this->bufferSize);
					if (!isConnected)
					{
#ifdef _WIN32
						Sleep(1000);
						goto reconnect;
#elif __LINUX_
						// TODO
						goto reconnect;
#endif
					}
					cout << "ServiceHost is connected to server !";
					this->RegisterService(this->serviceName);
				}
				string &GetJsonValue(cJSON *parentNode, char *name)
				{
					cJSON *node = cJSON_GetObjectItemCaseSensitive(parentNode, name);
					string val = node->valuestring;
					return val;
				}

				void InternalHandleCTCMessage(byte *buf, int messageLen)
				{
					byte sourceServiceNameLen = buf[0];
					string sourceServiceName;
					sourceServiceName.assign((char *)(buf + 1), sourceServiceNameLen);
					byte *msgBuf = new byte[messageLen - 1 - sourceServiceNameLen - 8];
					memcpy(msgBuf, buf + 1 + sourceServiceNameLen, messageLen - 1 - sourceServiceNameLen - 8);
						long tick = ToLong(buf, messageLen - 8);
					cJSON *p = cJSON_Parse((char *)msgBuf);
					string &dllPath = GetJsonValue(p, "AssemblyPath");
					string &methodName = GetJsonValue(p, "MethodName");
					cJSON *_params = cJSON_GetObjectItemCaseSensitive(p, "Params");
					string params = cJSON_Print(_params);
					cJSON_Delete(p);
                    RequestInfo info ;
					info.tick = tick ;
					info.BeginInvoke(dllPath,methodName,params,&this->SendingQueue) ;
                    this->RequestInfoQueue.push(info) ;
				}
				void InternalHandleCTCMessageNoLoop(byte *buf, int messageLen)
				{
					byte *msgBuf = new byte[messageLen - 8];
					memcpy(msgBuf, buf, messageLen - 8);
					long tick = ToLong(buf, messageLen - 8); // BitConverter.ToLong(buf, );
					handledResultBufferDic.insert(std::make_pair(tick, msgBuf));
				}

				void SpyingServiceChanged(byte *buf, int messageLen)
				{
				}
			};
#ifdef _WIN32
			DWORD WINAPI SendingQueueThread(LPVOID p)
			{
				ServiceHost *thisServer = (ServiceHost *)p;
				thisServer->SendingQueueLoop();
				return 0;
			}
				DWORD WINAPI ExecuteTaskQueueThread(LPVOID p)
			{
				ServiceHost *thisServer = (ServiceHost *)p;
				thisServer->SendingQueueLoop();
				return 0;
			}
#elif __linux__
	     void   SendingQueueThread(void * p)
			{
				ServiceHost *thisServer = (ServiceHost *)p;
				thisServer->SendingQueueLoop();
			 
			}
			void   ExecuteTaskQueueThread(void * p)
			{
				ServiceHost *thisServer = (ServiceHost *)p;
				thisServer->SendingQueueLoop();
				 
			}
#endif

		}
	}
}
#endif
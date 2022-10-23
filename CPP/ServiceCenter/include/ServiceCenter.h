#ifndef SERVICECENTER_H
#define SERVICECENTER_H
#include "TCPServer.h"
#include "../include/Helper.h"
namespace shikii
{
	namespace Hub
	{
		namespace Networking
		{
			class ServiceCenter : public TCPServer
			{
			public:
			protected:
				map<string, string> RegisteredServices;
				map<string, set<string>> RegisteredSpyingServices;
				map<string,int> RegisteredServiceProcessIds ;
				virtual void Route(byte msgKind, const char *szClientId, byte *buffer, int messageLen)
				{
					SOCKET sct = this->GetClientSocket(szClientId);
					switch (msgKind)
					{
					case shikii::Hub::Networking::Signals::REGISTER_SERVICE:
						RegisterService(szClientId, buffer, messageLen);
						break;
					case shikii::Hub::Networking::Signals::BYTES_CTC:
						PassMessageFromCTCBegin(szClientId, buffer, messageLen);
						break;
					case shikii::Hub::Networking::Signals::BYTES_CTC_NoLoop:
						PassMessageFromCTCEnd(szClientId, buffer, messageLen);
						break;
					case shikii::Hub::Networking::Signals::GET_REGISTERED_SERVICES:
						GetRegisteredServices(szClientId, buffer,messageLen);
						break;
						/*case shikii::Hub.Networking::Signals.UploadFileBegin:RecieveFileBegin(buf);break;
						case shikii::Hub.Networking::Signals.UploadingFile: RecievingFile(buf); break;
						case shikii::Hub.Networking::Signals.UploadFileEnd: RecievingFileEnd(buf); break;
						case shikii::Hub.Networking::Signals.DownloadFileRequest:this.SendingFile(clientId, buf); break;*/
					case shikii::Hub::Networking::Signals::RegisterSpyingService:
						this->RegisterSpyingServices(szClientId, buffer, messageLen);
						break; //你要监视的服务
					case shikii::Hub::Networking::Signals::GET_CONFIG_FILE_CONTENT:
                         GetConfigFileContent(szClientId, buffer,messageLen);
						break;
					case shikii::Hub::Networking::Signals::BootApp:
                         BootApp(szClientId, buffer,messageLen);
						break;
					case shikii::Hub::Networking::Signals::KillApp:
					      TerminateApp(szClientId, buffer,messageLen) ;
					      break;
					}
				}
               
			    void TerminateApp(string clientId, byte * buf,int messageLen)
				{
                   #ifdef _WIN32
				     //TerminateProcess()
				   #elif __linux__
				   #endif
				}
                void BootApp(string clientId, byte * buf,int messageLen)
				{
					string s  ;
                    #ifdef _WIN32
					cJSON * pJson = NULL ;
					try{
						string jsonStr ;
						jsonStr.assign((char*)buf, messageLen-8) ;
						  pJson = cJSON_Parse(jsonStr.c_str()) ;
						cJSON * pContent = cJSON_GetObjectItemCaseSensitive(pJson,"content") ;
						 string  strContent = pContent->valuestring ;
						  cJSON * pRealContent = cJSON_Parse(strContent.c_str()) ;
						string   bootAction = GetJsonValue(pRealContent,"bootAction") ;
						string    executableFilePath =GetJsonValue(pRealContent,"executableFilePath") ;
						string  bootExeArgs=GetJsonValue(pRealContent,"bootExeArgs") ;
						int displayMode=  cJSON_GetObjectItemCaseSensitive(pRealContent,"displayMode")->valueint ;
						ShellExecute(NULL,bootAction.c_str() , executableFilePath.c_str(),bootExeArgs.c_str(),NULL,displayMode );
						 cJSON_Delete(pRealContent) ;
						s = "{\"status\":\"success\",\"msg\":\"boot success !\"}" ;
					}
					catch(exception e)
					{
						  s = e.what() ;
						  cout<<s<<endl ;

					}
					this->SendEx(buf,messageLen,clientId,shikii::Hub::Networking::Signals::BootApp,(byte*)s.c_str(),s.size()) ;
					if(pJson != NULL)
					  cJSON_Delete(pJson) ;
					#endif
				}
              
			   void SendEx(byte * buf,int bufLen,string & clientId,byte msgKind,byte * content ,int contentSize)
			   {
				        int len = TCPBase::MARKPOSITION + contentSize + 8;
						byte *messageContent = new byte[len];
						memcpy(messageContent + TCPBase::MARKPOSITION + contentSize, buf+bufLen-8, 8);
						messageContent[0] = msgKind ;
						byte *lenBuf = this->GetBytes(len);
						memcpy(messageContent + 1, lenBuf, 4);
						memcpy(messageContent + TCPBase::MARKPOSITION, content, contentSize);
						SOCKET client = this->GetClientSocket(clientId);
						send(client, (char *)messageContent, len, 0);
						delete[] lenBuf;
						delete[] messageContent;
			   }

				void GetConfigFileContent(string clientId, byte * buf,int messageLen)
				{
					fstream fs;
					fs.open("bootableAssemblyList.json", ios::in|ios::binary);
					 byte * fileContent = NULL ;
					int contentSize = 0 ;
					string temp = "can't read config file" ;
					if (fs.is_open())
					{
						fs.seekg(0, std::ios::end);
					    int len = fs.tellg() ;
						contentSize = len ;
						fileContent = new byte[len] ;
						fs.seekg(0, std::ios::beg);
						fs.read((char*)fileContent,len);
						fs.close();
					}
					else 
					 {
						fileContent = (byte*)temp.c_str();
						contentSize = temp.size() ;
					 }
                     SendEx(buf,messageLen,clientId,shikii::Hub::Networking::Signals::GET_CONFIG_FILE_CONTENT,fileContent,contentSize) ;
					 delete [] fileContent ;
				}
					void PassMessageFromCTCEnd(string clientId, byte * buf, int messageLen)
					{
						int nLen = messageLen + TCPBase::MARKPOSITION;
						int targetServiceNameStrLen = buf[0];
						string targetServiceName;
						targetServiceName.assign((char *)(buf + 1), targetServiceNameStrLen);
						int newArraySize = nLen - targetServiceNameStrLen - 1;
						byte *data = new byte[newArraySize];
						byte *lenByts = this->GetBytes(newArraySize);
						data[0] = shikii::Hub::Networking::Signals::BYTES_CTC_NoLoop;
						memcpy(data + 1, lenByts, 4);
						byte *messageBuf = buf + 1 + targetServiceNameStrLen;
						memcpy(data + TCPBase::MARKPOSITION, messageBuf, messageLen - targetServiceNameStrLen - 1);
						if (RegisteredServices.find(targetServiceName) != RegisteredServices.end())
						{
							string targetClientId = RegisteredServices[targetServiceName];
							SOCKET sct = this->GetClientSocket(targetClientId);
							send(sct, (char *)data, newArraySize, 0);
						}
						delete[] lenByts;
						delete[] data;
					}
					void PassMessageFromCTCBegin(string clientId, byte * buf, int messageLen)
					{
						string sourceServiceName = InternalGetServiceName(clientId);
						byte *SourceServiceNameBytes = (byte *)sourceServiceName.c_str();
						int nLen = messageLen + TCPBase::MARKPOSITION;
						int targetServiceNameStrLen = buf[0];
						string targetServiceName;
						targetServiceName.assign((char *)(buf + 1), targetServiceNameStrLen);
						int newArraySize = nLen - targetServiceNameStrLen + sourceServiceName.size();
						byte *data = new byte[newArraySize];
						byte *lenByts = this->GetBytes(newArraySize);
						data[0] = shikii::Hub::Networking::Signals::BYTES_CTC;
						memcpy(data + 1, lenByts, 4);
						data[TCPBase::MARKPOSITION] = (byte)sourceServiceName.size();
						memcpy(data + TCPBase::MARKPOSITION + 1, SourceServiceNameBytes, sourceServiceName.size());
						byte *messageBuf = buf + targetServiceNameStrLen + 1;
						memcpy(data + TCPBase::MARKPOSITION + 1 + sourceServiceName.size(), messageBuf, messageLen - 1 - targetServiceNameStrLen);
						if (RegisteredServices.find(targetServiceName) != RegisteredServices.end())
						{
							string targetClientId = RegisteredServices[targetServiceName];
							SOCKET sct = this->GetClientSocket(targetClientId);
							send(sct, (char *)data, newArraySize, 0);
						}
						delete[] lenByts;
						delete[] data;
					}

					cJSON *GetStringNodeJson(const char *_nodeName, const char *_nodeContent, cJSON *node = NULL)
					{
						if (node == NULL)
							node = cJSON_CreateObject();
						cJSON *nodeContent = cJSON_CreateString(_nodeContent);
						cJSON_AddItemToObject(node, _nodeName, nodeContent);
						return node;
					}

                  cJSON * MakeNodeInfoJson(string name,int procId,cJSON* jsonBody)
				  {
					     cJSON *itemBody = GetStringNodeJson("Name", name.c_str());
						 #ifdef _WIN32
						 HANDLE   hProc = getProcById(procId) ;
						  int   numProcessors = 0 ;
						  ULARGE_INTEGER  lastCPU,  lastSysCPU,lastUserCPU ;
						 begineGetCurrentCPUUsage(hProc,numProcessors,lastCPU, lastSysCPU,lastUserCPU) ;
					     Sleep(50);
						double usage =  endGetCurrentCPUUsage(hProc,numProcessors,lastCPU, lastSysCPU,lastUserCPU) ;
						#elif __linux__
						 prepareMeasureCPUUsage() ;
					     Sleep(50);
						double usage =  getCurrentCPUUsage() ;
						#endif
						char *sz = new char[20];
						sprintf(sz, "%.2lf", usage);
						GetStringNodeJson("CPU Usage", sz, itemBody);
						#ifdef _WIN32
						double MemUsage = getCurrentMem(procId);
						#elif __linux__
						double MemUsage = getCurrentMem();
						#endif
						memset(sz, '\0', 20);
						sprintf(sz, "%.2lf MB", MemUsage / 1024 / 1024);
						GetStringNodeJson("Memory Usage", sz, itemBody);
						cJSON_AddItemToArray(jsonBody, itemBody);
						delete [] sz ;
						return itemBody ;
				  }

					void GetRegisteredServices(string clientId, byte * buf,int messageLen)
					{
						cJSON *jsonBody = cJSON_CreateArray();
						MakeNodeInfoJson("Service Center ( Powered By C++ )",0,jsonBody);
                         map<string,int>::iterator _it = RegisteredServiceProcessIds.begin() ;
						 
						while (_it !=  RegisteredServiceProcessIds.end())
						{
                             string serviceName = _it->first ;
							 int id = _it->second ;
							 cJSON* itemBody = MakeNodeInfoJson(serviceName,id,jsonBody);
							 cJSON_AddStringToObject(itemBody,"ClientId:",this->RegisteredServices[serviceName].c_str()) ;
							 cJSON * pSpyingServices = cJSON_CreateArray() ;
							 set<string> & spyingServices = this->RegisteredSpyingServices[serviceName] ;
                              set<string>::iterator  __it = spyingServices.begin() ;
							  while (__it != spyingServices.end())
							  {
							        string v = *__it  ;
									cJSON * subItem = cJSON_CreateString(v.c_str()) ;
									cJSON_AddItemToArray(pSpyingServices,subItem) ;
									__it++ ;
							  }
							  cJSON_AddItemToObject(itemBody,"Spying Services",pSpyingServices) ;
							_it++ ;
						}
						string json = cJSON_Print(jsonBody);
						byte *content = (byte *)json.c_str();
						this->SendEx(buf,messageLen,clientId,shikii::Hub::Networking::Signals::GET_CONFIG_FILE_CONTENT,content,json.size()) ;
						cJSON_Delete( jsonBody);
					}
					void RegisterService(string clientId, byte * buf, int messageLen)
					{
						string serviceName;
						cJSON * info = cJSON_Parse((char *)buf) ;
						cJSON * _serviceName = cJSON_GetObjectItemCaseSensitive(info,"Name") ;
						serviceName = _serviceName->valuestring ;
					    cJSON * _procId = cJSON_GetObjectItemCaseSensitive(info,"ProcId") ;
						int procId = _procId->valueint ; 
						if (RegisteredServices.find(serviceName) != RegisteredServices.end())
						{
							if(RegisteredServices.size() > 0 && RegisteredServices.find(serviceName) != RegisteredServices.end())
							{
                                   RegisteredServices.erase(serviceName);
							       this->RegisteredServiceProcessIds.erase(serviceName);
							}
							if (RegisteredServices.size() > 0 && RegisteredSpyingServices.find(serviceName) != RegisteredSpyingServices.end())
								RegisteredSpyingServices.erase(serviceName);
						}
						RegisteredServices.insert(std::make_pair(serviceName, clientId));
						this->RegisteredServiceProcessIds.insert(std::make_pair(serviceName,procId)) ;
						this->NotifyServiceChanged(serviceName, true);
						cout << "service:  \"" << serviceName << "\"  has registered!" << endl;
						cJSON_Delete( info) ;
					}

					void RegisterSpyingServices(string clientId, byte * buf, int messageLen)
					{

						string SourceServiceName = InternalGetServiceName(clientId);
						if(SourceServiceName.empty())
						 {
							return ;
						 }
						string rawserviceNames;
						rawserviceNames.assign((char *)buf, messageLen);
						vector<string> arr = split(rawserviceNames, ";");
						if (arr.size() >= 0)
						{
						checkIsContainsSourceServiceName:;
							bool isContainsSourceServiceName = this->RegisteredSpyingServices.find(SourceServiceName) != RegisteredSpyingServices.end();
							if (isContainsSourceServiceName)
							{
								set<string> &spyingServices = this->RegisteredSpyingServices[SourceServiceName];
								stringstream ss;
								ss << "{";
								for (int i = 0; i < arr.size(); i++)
								{
									spyingServices.insert(trim(arr[i]));
									if (this->RegisteredServices.find(trim(arr[i])) != RegisteredServices.end())
									{
										ss << "\"" << trim(arr[i]) << "\":true"
										   << ",";
									}
									else
									{
										ss << "\"" << trim(arr[i]) << "\":false"
										   << ",";
									}
								}
								string result = ss.str();
								result.pop_back();
								result.push_back('}');
								//发送已经断开连接的状态消息给相应的服务
								this->Send(clientId, shikii::Hub::Networking::Signals::SpyingServiceChanged, result);
							}
							else
							{
								cout<<"registed spying services: "<<SourceServiceName <<endl;
								this->RegisteredSpyingServices.insert(std::make_pair(SourceServiceName, *(new set<string>())));
								goto checkIsContainsSourceServiceName;
							}
						}
					}
					virtual void ClientDisconnectedCallback(const char *szClientId)
					{

						string serviceName = InternalGetServiceName(szClientId);
						cout << serviceName << " (" << szClientId
							 << ") is offline! " << endl;
						if (!serviceName.empty())
						{
							//如果订阅者服务断开了，则清除其已注册监视的服务列表
							if (RegisteredServices.size() > 0 && RegisteredSpyingServices.find(serviceName) != RegisteredSpyingServices.end())
								RegisteredSpyingServices.erase(serviceName);
							if (RegisteredServices.find(serviceName) != RegisteredServices.end())
							{
								RegisteredServices.erase(serviceName);
								this->RegisteredServiceProcessIds.erase(serviceName);
							}
							this->NotifyServiceChanged(serviceName, false);
						}
					}

					virtual void ClientConnectedCallback(const char *szClientId)
					{
						cout << szClientId << " "
							 << "has connected" << endl;
					}

					string InternalGetServiceName(string clientId)
					{
						string serviceName;
						for (map<string, string>::iterator it = RegisteredServices.begin(); it != RegisteredServices.end(); ++it)
						{
							// key
							string key = it->first;
							// value
							string val = it->second;
							if (val == clientId)
							{
								serviceName = key;
								break;
							}
						}
						return serviceName;
					}
					/// <summary>
					/// 如果有服务订阅了其它服务状态变更事件
					/// </summary>
					/// <param name="changedServiceName">服务状态变的服务名</param>
					/// <param name="status">服务状态 true--connect false--disconnect</param>
					void NotifyServiceChanged(string changedServiceName, bool status)
					{
						//如果有服务订阅了其它服务状态变更事件
						if (RegisteredSpyingServices.size() > 0)
						{
							map<string, set<string>>::iterator it = RegisteredSpyingServices.begin();
							string currentNodeName = "" ;
							while (it != RegisteredSpyingServices.end())
							{
								currentNodeName = it->first ;
								set<string> &spyingService = it->second;
								if (spyingService.empty())
									continue;
									int n = spyingService.size() ;
									if(n < 0)
									 {
										spyingService.clear() ;
										it++;
										continue;
									 }

								if (spyingService.find(changedServiceName) != spyingService.end())
								{
									//取出订阅者服务名
									string hostServiceName = it->first;
									//检查订阅者服务是否在线
									if (this->RegisteredServices.find(hostServiceName) != RegisteredServices.end())
									{
										string _clientId = this->RegisteredServices[hostServiceName];
										stringstream ss;
										if (status)
											ss << "{\"" << changedServiceName << "\":true"
											   << "}";
										else
											ss << "{\"" << changedServiceName << "\":false"
											   << "}";

										//发送已经断开连接的状态消息给相应的服务
										this->Send(_clientId, shikii::Hub::Networking::Signals::SpyingServiceChanged, ss.str());
									}
								}
								it++;
							}
						}
					}

				private:
				};
			}
		}
	}

#endif
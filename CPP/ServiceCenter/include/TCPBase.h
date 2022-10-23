#ifndef TCPBASE_H
#define TCPBASE_H
#ifdef _WIN32
#include <WinSock2.h>
#include <Windows.h>
#elif __linux__
#include <unistd.h>
#include <sys/types.h>
#include <sys/socket.h>
#include <netdb.h>
#include <memory.h>
#include <signal.h>
#include <time.h>
#include <pthread.h>
#include <errno.h>
#include <arpa/inet.h>
typedef unsigned char  byte ;
typedef int SOCKET ;
#endif
#include <iostream>
#include <fstream>
#include <vector>
#include <map>
#include <set>
#include <algorithm>
#include <exception>
#include "Signals.h"
#include <sstream>
#include <thread>
#include "cJSON.h"

#ifdef _WIN32
#pragma comment(lib, "Ws2_32.lib")
#endif
using namespace std;
namespace shikii
{
	namespace Hub
	{
		namespace Networking
		{
			class TCPBase
			{
			public:
				string Ip;
				int Port;
				int SystemBufferSize;
				static const int MARKPOSITION = 5;
			
				TCPBase()
				{
				
					bEndNetwork = false;
					this->bufferSize = 8192;
					#ifdef _WIN32
					wVersionRequested = MAKEWORD(2, 2);
					//需要确认是否初始化成功
					if (WSAStartup(this->wVersionRequested, &this->wsaData) != 0)
					{
						return;
					}
					#endif
				}
                const char* GetJsonValue(cJSON *parentNode, char *name)
				{
					cJSON *node = cJSON_GetObjectItemCaseSensitive(parentNode, name);
					return  node->valuestring;
				}
               #ifdef __linux__
			        void Sleep(int milsecs)
					 {
						usleep (milsecs * 1000) ;
					 }
			   #endif 

				/*
				    codePage 为数字 936 时代表是 GB2312
				*/
			#ifdef _WIN32
					template <typename T, typename X>
					X *GetTString(T * pSource, int nLen,int codePage = CP_UTF8)
					{
						int whichType = sizeof(T);
						int isize = 0;
						switch (whichType)
						{
						case 1:
							isize = MultiByteToWideChar(codePage, 0, pSource, nLen, NULL, 0, NULL, NULL);
							wchar_t *pwchars = new wchar_t[isize];
							MultiByteToWideChar(codePage, 0, pSource, nLen, NULL, 0, pwchars, NULL);
							return pwchars;

						case 2:
							isize = WideCharToMultiByte(codePage, 0, pSource, nLen, NULL, 0, NULL, NULL);
							char *pchars = new char[isize];
							WideCharToMultiByte(codePage, 0, pSource, nLen, NULL, 0, pchars, NULL);
							return pchars;
						}
					}
			#endif		
				

			protected:
				bool bEndNetwork;
				int bufferSize;
				//套接字地址 (IP Port)
				sockaddr_in sockAddr;
				#ifdef _WIN32
				////版本号
				WORD wVersionRequested;
				// winsock数据
				WSADATA wsaData;
				#endif
				void SetSystemBufferSize(SOCKET sct)
				{
					setsockopt(sct, SOL_SOCKET, SO_RCVBUF, (const char *)&this->SystemBufferSize, sizeof(int));
					setsockopt(sct, SOL_SOCKET, SO_SNDBUF, (char *)&this->SystemBufferSize, sizeof(int));
				}
				//在收发数据的时，希望不经历由系统缓冲区到socket缓冲区的拷贝而影响
				//程序的性能
				void DisableSystemBuffer(SOCKET sct)
				{
					int nZero = 0;
					setsockopt(sct, SOL_SOCKET, SO_SNDBUF, (char *)&nZero, sizeof(nZero));
					setsockopt(sct, SOL_SOCKET, SO_RCVBUF, (char *)&nZero, sizeof(int));
				}

				void Prepare(string ip = "127.0.0.1", int port = 8040)
				{
					this->Ip = ip;
					this->Port = port;
					memset(&sockAddr, 0, sizeof(sockAddr));			  //用0填充每个字节
					sockAddr.sin_family = PF_INET;					  //使用PF_INET地址族，也就是IPv4
					sockAddr.sin_addr.s_addr = inet_addr(ip.c_str());//htonl(INADDR_ANY);//(ip.c_str()); //具体的地址
					sockAddr.sin_port = htons(port);				  //端口
				}
			 
				int FetchDataLenByts(byte *buffer)
				{
					byte *byt_Len = new byte[4];
					for (int i = 1; i < MARKPOSITION; i++)
					{
						byt_Len[i - 1] = buffer[i];
					}
					int byteNum = ToInt(byt_Len, 0);
					delete[] byt_Len;
					return byteNum;
				}

				int ToInt(byte *buf, int startIndex)
				{
					byte *nArr = new byte[4];
					int n = 0;
					int count = 4 + startIndex;
					for (int i = startIndex; i < count; i++)
					{
						if (buf[i] < 0)
						{
							nArr[i - startIndex] = buf[i] & 0xFF;
						}
						else
							nArr[i - startIndex] = buf[i];
					}

					for (int i = 0; i < 4; i++)
					{
						n = n | (nArr[i] & 0xFF) << 8 * i;
					}
					delete[] nArr;
					return n;
				}

             long ToLong(byte *buf, int startIndex)
				{
					int nLen = 8 ;
					byte *nArr = new byte[nLen];
					int count = nLen + startIndex;
					for (int i = startIndex; i < count; i++)
					{
						if (buf[i] < 0)
						{
							nArr[i - startIndex] = buf[i] & 0xFF;
						}
						else
							nArr[i - startIndex] = buf[i];
					}
                   long n = 0;
					for (int i = 0; i < nLen; i++)
					{
						n = n | (nArr[i] & 0xFF) << 8 * i;
					}
					delete[] nArr;
					return n;
				}


				void CopyTo(byte *bytArr_Src, int count_src, byte *bytArr_Dst,
							int nIndexDstStart)
				{
					for (int i = 0; i < count_src; i++)
					{
						bytArr_Dst[i + nIndexDstStart] = bytArr_Src[i];
					}
				}
				byte *GetBytes(int n)
				{
					byte *targets = new byte[4];
					targets[0] = (byte)(n & 0xff);		   // 最低位
					targets[1] = (byte)((n >> 8) & 0xff);  // 次低位
					targets[2] = (byte)((n >> 16) & 0xff); // 次高位
					targets[3] = (byte)(n >> 24);		   // 最高位,无符号右移。
					return targets;
				}

				vector<string>  split(const string &str, const string &delim)
				{
					vector<string> res;
					if ("" == str)
						return res;
					//先将要切割的字符串从string类型转换为char*类型
					char *strs = new char[str.length() + 1]; //不要忘了
					strcpy(strs, str.c_str());

					char *d = new char[delim.length() + 1];
					strcpy(d, delim.c_str());

					char *p = strtok(strs, d);
					while (p)
					{
						string s = p;	  //分割得到的字符串转换为string类型
						res.push_back(s); //存入结果数组
						p = strtok(NULL, d);
					}

					return res;
				}
				std::string &trim(std::string &s)
				{
					if (s.empty())
					{
						return s;
					}
					s.erase(0, s.find_first_not_of(" "));
					s.erase(s.find_last_not_of(" ") + 1);
					return s;
				}

		   ~TCPBase()
			   {
				#ifdef _WIN32
                  WSACleanup();
				  #endif
			   }
			 
 
			};
		}
	}
}
#endif
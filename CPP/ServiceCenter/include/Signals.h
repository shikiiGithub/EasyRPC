#ifndef SIGNALS_H
#define SIGNALS_H
#ifdef _WIN32
#include <windows.h>
#endif
#include <string.h>
using namespace std ;
namespace shikii{
	namespace Hub{
		namespace Networking
		{
			#ifdef _WIN32
				typedef const char *(__cdecl *WatchDog)(void *,void *);
			#endif 	
				
			class Signals
			{
			public:
				static const unsigned char BYTES_CTS = 0;
				static const unsigned char BYTES_STC = 1;
				static const unsigned char BYTES_CTC = 2;
				static const unsigned char BYTES_CTC_NoLoop = 3;
				static const unsigned char _FILE_BEGIN = 4;
				static const unsigned char FILE_TRANSFER = 5;
				static const unsigned char _FILE_END = 6;
				static const unsigned char DOWNLOAD_FILE = 7;
				static const unsigned char REGISTER_SERVICE = 8;
				static const unsigned char GET_REGISTERED_SERVICES = 9;
				static const unsigned char UploadFileBegin = 10;
				static const unsigned char UploadingFile = 11;
				static const unsigned char UploadFileEnd = 12;
				static const unsigned char DownloadFileRequest = 13;
				static const unsigned char DownloadFileBegin = 14;
				static const unsigned char DownloadingFile = 15;
				static const unsigned char DownloadFileEnd = 16;
				static const unsigned char NodeJSWebAPI = 17;

				//注册要监视的服务
				//主要是看服务是否有变化 （连接断开）
				static const unsigned char RegisterSpyingService = 18;
				/// <summary>
				/// 当所监视的服务变化时（连接/断开）
				/// </summary>
				static  const unsigned char  SpyingServiceChanged = 19;
                static  const unsigned char  GET_CONFIG_FILE_CONTENT = 20;
				static  const unsigned char   BootApp = 21;
				static  const unsigned char   KillApp = 22;
				//执行特定的方法
				static  const unsigned char  CALL_METHOD = 51;

			};
		    class CTCMessage{
                string AssemblyPath ; //dll 路径
                string AssemblyName;  //在CPP 中不适用
				/**
				 * 注意是 包名 + 类名
				 */
               string ClassName; //在CPP 中不适用
               string MethodName ; 
               void *  Params ; 
               string  ReturnedData  ;
               string ErrorMsg ;
			} ;
		//  class DaemonThreadInfo {
		// 	public:
		// 	bool  IsBusy ;
		// 	#ifdef _WIN32
		// 	HANDLE ThisThread  ;
		// 	string ThreadId ;
		// 	#endif
		// 	void*  Tag ;
        //   } ;
         #ifdef _WIN32
		 	class DllLibrary
			{
			public:
				bool Link(const char *pszDllName)
				{
					dllName = pszDllName ;
			 
					hInstLibrary = LoadLibrary(dllName.c_str());
					if (hInstLibrary == NULL)
					{
						FreeLibrary(hInstLibrary);
						return false;
					}
					else
						return true;
				}
				/*
					 typedef  HANDLE(*XL_CreateTaskByURL)(const wchar_t *url, const wchar_t *path, const wchar_t *fileName, BOOL IsResume);
					 XL_CreateTaskByURL createTask;
					 bool isCreatedTask = dllManager.LocateFuction(&createTask, "XL_CreateTaskByURL");
				*/
				template <typename T>
				bool LocateFuction(T *pFun, const char *pszFunctionName)
				{
					*pFun = (T)GetProcAddress(hInstLibrary, pszFunctionName);
					if (pFun == NULL)
					{
						FreeLibrary(hInstLibrary);
						return false;
					}
					return true;
				}
				bool Disconnect()
				{

					return FreeLibrary(hInstLibrary);
				}

			protected:
			 string	dllName ;
				HINSTANCE hInstLibrary;
			};
		   class RequestInfo {
			public : 
			  DllLibrary dllMan ;
			  string params ;
			  WatchDog pFun;
			  long tick ;
			  void * sendingQueue ;
			  bool BeginInvoke(string & dllPath,string & methodName,string & params,void * sendingQueue)
			  {
				   bool  isSuccess = dllMan.Link(dllPath.c_str()) ;
				   this->params = params ; 
				   this->sendingQueue = sendingQueue ;
				   if(isSuccess)
				    isSuccess = dllMan.LocateFuction(&pFun, methodName.c_str());
				    return isSuccess ;
			  }
			  string & Invoke( )
			  { 
					string tmp =  (char*)pFun((void*)this->params.c_str(),sendingQueue);
					return tmp ;
			  }
			  ~RequestInfo(){
				dllMan.Disconnect() ;
			  }
		  } ;
		 #elif __linux__

		  class RequestInfo {
			public : 
			 
			  string params ;
			 
			  long tick ;
			  void * sendingQueue ;
			  bool BeginInvoke(string & dllPath,string & methodName,string & params,void * sendingQueue)
			  {
				//    bool  isSuccess = dllMan.Link(dllPath.c_str()) ;
				//    this->params = params ; 
				//    this->sendingQueue = sendingQueue ;
				//    if(isSuccess)
				//     isSuccess = dllMan.LocateFuction(&pFun, methodName.c_str());
				//     return isSuccess ;
			  }
			  string & Invoke( )
			  { 
					// string tmp =  (char*)pFun((void*)this->params.c_str(),sendingQueue);
					// return tmp ;
			  }
			  ~RequestInfo(){
				// dllMan.Disconnect() ;
			  }
		  } ;
		 #endif
		
 


		} 
		

	}

} 
	
#endif
 

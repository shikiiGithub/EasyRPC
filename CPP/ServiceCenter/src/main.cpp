#include "../include/ServiceCenter.h"
#include <streambuf>
#ifdef _WIN32
#include "../include/ServiceHost.h"
#endif
 
bool BootByConfig(bool server,
char   *argv[],
string configFilePath) ;
#ifdef _WIN32
int main(int argc, char * argv[])
#elif __linux
int main(int argc, char *argv[])
#endif 
{
	bool isServerMode = false ;
	
     if(argc == 1)
	  isServerMode = true ;
	  string filePath = "bootableAssemblyList.json" ;
	 BootByConfig(isServerMode,argv,filePath);
	 #ifdef _WIN32
	 system("pause") ;
	 #elif __linux__
	 int userInput = 0 ;
	  cin>>userInput ;
	 #endif
}
bool BootByConfig(bool server,
char  *argv[],
string configFilePath)
{	
	fstream fs ;
	  fs.open(configFilePath,ios::in) ;
	  if(fs.is_open())
	  {
		  std::string contents;
		  fs.seekg(0, std::ios::end);
		  contents.resize(fs.tellg());
		  fs.seekg(0, std::ios::beg);
		  fs.read(&contents[0], contents.size());
		  fs.close();
		  cJSON *cfg_json = cJSON_Parse(contents.c_str());
		  if (cfg_json == NULL)
		  {
			  const char *error_ptr = cJSON_GetErrorPtr();
			  if (error_ptr != NULL)
			  {
				  fprintf(stderr, "Error before: %s\n", error_ptr);
			  }
			  return false ;
		  }
		cJSON * serviceCenterNode =  cJSON_GetObjectItemCaseSensitive(cfg_json, "serviceCenter");
		cJSON * _port  = cJSON_GetObjectItemCaseSensitive(serviceCenterNode,"port") ;
		int port =  _port->valueint ;
		cJSON * _ip  = cJSON_GetObjectItemCaseSensitive(serviceCenterNode,"ip") ;
		string ip = _ip->valuestring ;
		cJSON * _bufferSize  = cJSON_GetObjectItemCaseSensitive(serviceCenterNode,"bufferSize") ;
		int bufferSize = _bufferSize->valueint ;
		cJSON_Delete(cfg_json);
		cout << "The config details is  ip:" <<ip<<" port:"<<port<<" buffer size:"<<bufferSize<<" bytes" <<endl ;
		 
		if(server)
		{
			shikii::Hub::Networking::ServiceCenter * pServiceCenter = new shikii::Hub::Networking::ServiceCenter() ;
			pServiceCenter->Boot(ip,port,bufferSize) ;
		}
		else 
		{
			#ifdef _WIN32
			shikii::Hub::Networking::ServiceHost * pHost = new  shikii::Hub::Networking::ServiceHost() ;
			bool isConnected = pHost->Connect(ip,port,bufferSize) ;
			reconnect:;
			if(!isConnected)
               isConnected= pHost->Reconnect(ip,port,bufferSize) ;
			if(!isConnected)
			 {
				#ifdef _WIN32
				  Sleep(1000) ;
                  goto reconnect ;
				#elif __LINUX_
				  //TODO 
				  goto reconnect ;
				#endif 
			 }
			 cout << "ServiceHost is connected to server !" ;
			 pHost->RegisterService(argv[1]) ;
			 #endif
		}
       
		return true ;
	  }
	  else 
	  {
		  cout<<"\r\nError: can't read config from config file , does \"bootableAssemblyList.json\" file exists ? "<<endl;
		  return false;

	  }
	 
	 
}
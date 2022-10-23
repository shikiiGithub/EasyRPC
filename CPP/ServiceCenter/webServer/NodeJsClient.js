
const ServiceHost = require('./ServiceHost')
const fs = require("fs");
class NodeJsClient{
      serviceCenterIP = "127.0.0.1" ;
      serviceCenterPort = 8040 ;
      prepared = false ;
      _LogService = "LogTower" ;
     _LogServiceOnline = false ;
     conf = null ;
    /**
     * @type {ServiceHost}
     */
     thisHost = null;
     constructor()
     {
        this.conf = this.GetConfigContentJson() ;
     }

       GetConfigContentJson = ()=>{
        try {
            let txt =   fs.readFileSync('../bootableAssemblyList.json', 'utf8');
            let conf = JSON.parse(txt) ;
            this.prepared = true ;
             this.thisHost = new ServiceHost();
            console.log("已读取到 bootableAssemblyList.json") ;
            return conf ;
            
        } catch (error) {
            this.prepared = false ;
            console.log("未能读取到 bootableAssemblyList.json") ;
        }
           
       }

        Connect=()=>
        {
            this.thisHost.connect(this.serviceCenterIP,this.serviceCenterPort) ;
        }
        /**
         * 
         * @param {[]} spyingServices 
         * @param {string} serviceName 
         */
        prepareEvent =(spyingServices,serviceName,onServiceChanged,onDisconnect)=>
        {
            
            this.serviceCenterIP = this.conf.serviceCenter.ip ;
            this.serviceCenterPort = this.conf.serviceCenter.port ;
            spyingServices.push(this._LogService) ;
            let spyingServiceString = '' ;
            for (let index = 0; index < spyingServices.length; index++) {
                const element = spyingServices[index];
                if(index < spyingServices.length -1)
                 {
                    spyingServiceString = spyingServiceString +element + ";" ;
                }
                else
                 {
                    spyingServiceString +=element ;
                 }
            }
            this.thisHost.serviceName = serviceName ;
            this.thisHost.spyingServices = spyingServiceString;
             let that = this ;
            this.thisHost.onConnected = ()=>{
                console.log('注册需要监视的服务')
                that.thisHost.register(that.thisHost.serviceName) ;
                console.log(that.thisHost.spyingServices)
                setTimeout(() => {
                    that.thisHost.registerSpyingServices(that.thisHost.spyingServices);
                }, 500);
               
                }
               this.thisHost.onDisconnect = ()=>{
                that._LogServiceOnline = false ;
                 onDisconnect();
                }
                this.thisHost.watchedServiceChanged = ( serviceStatusInfo)=>{
                    console.warn('监测到服务变动')
                    console.log(serviceStatusInfo)
                     let status = serviceStatusInfo[that._LogService] ;
                        if(status !== undefined && status !== null) {
                            that._LogServiceOnline = status;
                            if(that._LogServiceOnline)
                            {
                                that.LogInfo("NodeJs Client has been linked with ServiceCenter")
                            }
                        }
                         onServiceChanged(serviceStatusInfo) ;
                }
        }
        LogInfo = (text)=> {
        let  info = {};
        info.ServiceName = this.thisHost.serviceName;
        info.Level="INFO";
        info.Content = text ;
         let date =new Date() ;
         let years = date.getFullYear() ;
         let months = ( date.getMonth() + 1).toString().padStart(2,'0');
         let days = date.getDate().toString().padStart(2,'0');
         let hours = date.getHours().toString().padStart(2,'0');
         let minutes = date.getMinutes().toString().padStart(2,'0');
         let secs = date.getSeconds().toString().padStart(2,'0');
        info.FireDate = `${years}-${months}-${days}`;
        info.FireTime = `${hours}:${minutes}:${secs}`;
        let jsonStr = JSON.stringify(info) ;
        let message =  this.thisHost.getCTCMessage("LogTower","App","Info",jsonStr);
       this.thisHost.callService(this._LogService,message,
           (msg)=>{
    
           }
       ) ;
    }
    
      LogError = (text)=> {
       let  info = {};
       info.ServiceName =this.thisHost.serviceName;
       info.Level="ERROR";
       info.Content = text ;
       let date =new Date() ;
       let years = date.getFullYear() ;
       let months = ( date.getMonth() + 1).toString().padStart(2,'0');
       let days = date.getDate().toString().padStart(2,'0');
       let hours = date.getHours().toString().padStart(2,'0');
       let minutes = date.getMinutes().toString().padStart(2,'0');
       let secs = date.getSeconds().toString().padStart(2,'0');
       info.FireDate = `${years}-${months}-${days}`;
       info.FireTime = `${hours}:${minutes}:${secs}`;
       let jsonStr = JSON.stringify(info) ;
       let message =  this.thisHost.getCTCMessage("LogTower","App","Info",jsonStr);
       this.thisHost.callService(this._LogService,message,
           (msg )=>{
    
           }
       ) ;
}
}

module.exports = NodeJsClient;
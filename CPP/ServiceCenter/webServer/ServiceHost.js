
const net = require('net');
const { pid } = require('process');
class CTCMessage {
    AssemblyPath
    AssemblyName
    ClassName
    MethodName
    /**
     * @type {[]}
     */
    Params
    ReturnedData
    AliasDiName
    /**
     *
     * @type {number}
     */
    LifeCyleTimeMode = 1
    /**
     * @type {String}
     */
    ErrorMsg = null
}

class ServiceHost {
    /**
     * @type {net.Socket}
     */
    _client = null;
    host = null;
    port = null;
    connected = false ;
    respondQueque=[];
    watchedServiceChanged=null;
    onConnected = null;
    onDisconnect=null;
    registeredTypes=[];
    availableBuffers=[];
     /**
     *
     * @type {string}
     */
    serviceName = "";
     /**
     *
     * @type {string}
     */
    spyingServices = "" ;
    /**
     *
     * @type {string}
     */
    encoding = 'UTF-8';
    MessageBuffers = [];

    constructor() {
        this._client = new net.Socket();
        let that = this ;
        
        that._client.on('data', this.Route);
        this._client.on('error', function (error) {
            console.log('error:' + error);
        });
        this._client.on('connect',  ( )=> {
            that.onConnectedToServiceCenter();
        });
        this._client.on('close', function () {
            console.log('服务器端下线了');
            that.connected = false ;
          
            setTimeout(()=>{
                if(!that.connected)
                    that.connected = that._client.connect(that.port, that.host );
            },2000)
            if(that.onDisconnect)
            {
                that.onDisconnect();
            }
        });
    }

     Route = (msg)=> {
        let buf= new Buffer.from(msg, "binary") ;
        this.InferMessage(buf) ;
    }

   InferMessage = (buf)=>{
    let that = this ;
    let msgMark = buf.readUint8(0) ;
    let nTotalLen = buf.readUInt32LE(1) ;
    let currentMsgBuf = buf ;
    if(nTotalLen < buf.length)
           currentMsgBuf = buf.subarray(0,nTotalLen) ;
    switch (msgMark) {
        case that.BYTES_CTC_NoLoop :
            that.internalHandleCTCMessageNoLoop(currentMsgBuf);
            break;
        case that.SpyingServiceChanged:
            if(that.watchedServiceChanged)
            {
                try {
                     let nTotalLen = currentMsgBuf.readUInt32LE(1) ;
                     let __buf = currentMsgBuf.subarray(5) ;
                    let rawString = __buf.toString(this.encoding);
                    console.log("Json:"+rawString);
                    let serviceStatusInfo = JSON.parse(rawString) ;
                    that.watchedServiceChanged(serviceStatusInfo) ;
                    
                }
                catch (e) {
                    let __buf =  buf.subarray(5);
                    let rawString = __buf.toString(this.encoding);
                    console.log("错误Json:"+rawString);
                    console.error(e)
                }

            }
            break;
        case that.BYTES_CTC:that.internalHandleCTCMessage(currentMsgBuf);
        case that.GET_REGISTERED_SERVICES:this.onFromServiceCenterMsg(currentMsgBuf) ;
        case that.GET_CONFIG_FILE_CONTENT:this.onFromServiceCenterMsg(currentMsgBuf) ;
        case that.BootApp:this.onFromServiceCenterMsg(currentMsgBuf) ;
    }

    if(nTotalLen < buf.length)
           this.InferMessage( buf.subarray(nTotalLen))
   }

     getServiceInfo = ()=>{
        var ps = require('current-processes');
        
        ps.get(function(err, processes) {
            console.log(processes);
        });

        let mem = process.memoryUsage();
         
        let memUsage = mem.rss/1024.0/1024.0 ;
        memUsage = memUsage.toFixed(2);
     }
    onFromServiceCenterMsg = (buf) =>{
        let nLen = buf.readUInt32LE(1);
        let __buf =  buf.subarray(5,nLen);
        let _taskId = buf.readBigUInt64LE(nLen - 8)
        let index = -1 ;
        for (let i = 0; i < this.respondQueque.length; i++) {
            if(this.respondQueque[i].id === _taskId)
            {
                index = i ;
                break ;
            }
        }
        if( index !== -1)
        {
            let taskInfo = this.respondQueque[index] ;
            taskInfo.handled = true ;
            let resultBuffer = __buf.subarray(0,nLen-5 - 8);
            let resultStr = resultBuffer.toString(this.encoding);
            //let _msg = JSON.parse(resultStr);
            taskInfo.handler(resultStr);
            this.respondQueque.splice(index,1);
        }
    }

    onConnectedToServiceCenter=()=>{
        console.log('已连接到服务中心');
        this.connected = true;
        if(this.onConnected)
        {
            this.onConnected();
        }
    }
    connect = (host, port) => {
        this._client.setEncoding('binary');
        let that = this;
        this.host = host;
        this.port = port;
        this._client.connect(port, host);
    }

    register = (serviceName) => {
        console.log(`This process id is  ${pid}`) ;
        let msgMark = this.REGISTER_SERVICE;
        let _headBuffer = Buffer.alloc(5);
        _headBuffer.writeUint8(msgMark);
        let rawjson = {
            Name:serviceName,
            ProcId:pid 
        } ;
        let  json= JSON.stringify(rawjson) ;
        let contentBuffer = Buffer.from(json,this.encoding);
        let nlen = contentBuffer.length;
        nlen = nlen + 5;
        _headBuffer.writeUInt32LE(nlen, 1);
        let newBuffer = Buffer.concat([_headBuffer, contentBuffer]);
        this._client.write(newBuffer);
    }
    /**
     * 注册监视服务是否在线事件,请注意服务名与服务之间用英文分号分隔
     * @param serviceName {String}
     */
    registerSpyingServices = (serviceName) => {
        let msgMark = this.RegisterSpyingService;
        let _headBuffer = Buffer.alloc(5);
        _headBuffer.writeUint8(msgMark);
        let json = serviceName;
        let contentBuffer = Buffer.from(json,this.encoding);
        let nlen = contentBuffer.length;
        nlen = nlen + 5;
        _headBuffer.writeUInt32LE(nlen, 1);
        let newBuffer = Buffer.concat([_headBuffer, contentBuffer]);
        this._client.write(newBuffer);
    }
    getCTCMessage = (moduleName, className, methodName, ..._params) => {
        let message = new CTCMessage();
        message.AssemblyName = moduleName;
        message.ClassName = className;
        message.Params = _params;
        message.MethodName = methodName;
        return message;
    }


    /**
     *
     * @param serviceName {String}
     * @param message {CTCMessage}
     * @param onResponed {(msg)=>{}} CTCMessage
     */
    callService = (serviceName, message,onResponed  ) => {
        let that = this ;
        let jsonStr = JSON.stringify(message);
        let buf = Buffer.from(jsonStr,this.encoding );
        let serviceNameBytes = Buffer.from(serviceName,this.encoding );
        let nLen = 5 + serviceNameBytes.length + buf.length + 1 + 8;
        let _headbuffer = Buffer.alloc(6);
        _headbuffer.writeUint8(this.BYTES_CTC, 0);
        _headbuffer.writeUInt32LE(nLen, 1);
        _headbuffer.writeUint8(serviceNameBytes.length, 5);
        let _tailBuffer = Buffer.alloc(8);
        let taskId =BigInt(Date.now());
        console.log(taskId);
        _tailBuffer.writeBigInt64LE(taskId, 0);
        let newBuffer = Buffer.concat([_headbuffer, serviceNameBytes, buf, _tailBuffer]);
        this._client.write(newBuffer);
        this.respondQueque.push({id:taskId,handler:onResponed,handled:false});
    }

    fromServiceCenter=(onResponed,msgKind,extra)=>{
       
        if(extra == null || extra == undefined ||extra ==='')
        {
            let nLen = 5+8 ;
            let _headbuffer = Buffer.alloc(5);
            _headbuffer.writeUint8(msgKind, 0);
            _headbuffer.writeUInt32LE(nLen, 1);
            let _tailBuffer = Buffer.alloc(8);
            let taskId =BigInt(Date.now());
            console.log(taskId);
            _tailBuffer.writeBigInt64LE(taskId, 0);
            let newBuffer = Buffer.concat([_headbuffer, _tailBuffer]);
            this._client.write(newBuffer);
            this.respondQueque.push({id:taskId,handler:onResponed,handled:false});
        }
        else 
         {
            
            let nLen = 5+8 ;
            let buf = Buffer.from(extra,this.encoding );
            nLen+= buf.length ;
            let _headbuffer = Buffer.alloc(5);
            _headbuffer.writeUint8(msgKind, 0);
            _headbuffer.writeUInt32LE(nLen, 1);
            let _tailBuffer = Buffer.alloc(8);
            let taskId =BigInt(Date.now());
            console.log(taskId);
            _tailBuffer.writeBigInt64LE(taskId, 0);
            let newBuffer = Buffer.concat([_headbuffer,buf, _tailBuffer]);
            this._client.write(newBuffer);
            this.respondQueque.push({id:taskId,handler:onResponed,handled:false});
         }
       
    }

     /**
     * get all service from service center
     * @param onResponed {(msg)=>{}} msg from service center
     */
    getAllService = (onResponed)=>{
      this.fromServiceCenter(onResponed,this.GET_REGISTERED_SERVICES) ;
    }

    getConfigFileContent=(onResponed)=>{
      this.fromServiceCenter(onResponed,this.GET_CONFIG_FILE_CONTENT) ;
    }
    /**
     *
     * @param buf {Buffer}
     * @constructor
     */
    internalHandleCTCMessageNoLoop = (buf) => {
        let nLen = buf.readUInt32LE(1);
        let __buf =  buf.subarray(5,nLen);
        let _taskId = __buf.readBigUInt64LE(__buf.length - 8)
        let index = -1 ;
        for (let i = 0; i < this.respondQueque.length; i++) {
            if(this.respondQueque[i].id === _taskId)
            {
                index = i ;
                break ;
            }
        }
        if( index !== -1)
        {
            let taskInfo = this.respondQueque[index] ;
            taskInfo.handled = true ;
            let resultBuffer = __buf.subarray(0,__buf.length - 8);
            let resultStr = resultBuffer.toString(this.encoding);
            let _msg = JSON.parse(resultStr);
            taskInfo.handler(_msg);
            this.respondQueque.splice(index,1);
        }
    }

    /**
     *
     * @param buf {Buffer}
     */
    internalHandleCTCMessage = (buf) => {
        let len = buf.readUint32LE(1);
        let sourceServiceNameLen = buf.readUint8(5);
        let sourceServiceName = buf.toString(this.encoding, 5 + 1, sourceServiceNameLen);
        let msgBuf = buf.subarray(5 + sourceServiceNameLen + 1, len);
        let result = null;
        let timeTicks = msgBuf.subarray(msgBuf.length - 8);
        let realbuf = msgBuf.subarray(0, msgBuf.length - 8);
        let tempByts = this.internalHandleCTCMessageBegin(realbuf);
        result = Buffer.concat([tempByts, timeTicks]);
        if (result != null) {
            let bufLen = 5 + sourceServiceNameLen + 1 + result.length;
            let headBuffer = Buffer.alloc(6);
            headBuffer.writeUint8(this.BYTES_CTC_NoLoop);
            headBuffer.writeUInt32LE(bufLen, 1);
            headBuffer.writeUint8(sourceServiceNameLen, 5);
            let sourceServiceNameBytes = buf.subarray(5 + 1, 5 + 1 + sourceServiceNameLen)
            let newResult = Buffer.concat([headBuffer, sourceServiceNameBytes, result])
            this._client.write(newResult);

        }

    }

    /**
     *
     * @param data {Buffer}
     * @returns {Buffer}
     */
    internalHandleCTCMessageBegin=(data)=>
    {
        /**
         *
         * @type {CTCMessage}
         */
        let messageEntity = null;
        try {
            let str = data.toString(this.encoding);
            if (str === null || str === undefined)
                str = "";
            messageEntity = JSON.parse(str);
            if (messageEntity == null) {

                messageEntity = new CTCMessage();
                messageEntity.ErrorMsg = "所传数据格式不正确";
            }

            //如果调用的是模块程序集
            if (messageEntity.AssemblyName && messageEntity.AssemblyName !== "") {
                if(messageEntity.ClassName && messageEntity.ClassName !=='')
                {
                    let info = null ;
                    for (let i = 0; i < this.registeredTypes.length; i++) {
                        if(this.registeredTypes[i].className === messageEntity.ClassName)
                        {
                            info = this.registeredTypes[i];
                            break;
                        }
                    }
                    if(!info)
                    {
                        let _ClassType = null ;

                        if(messageEntity.ClassName.indexOf('.') ===-1)
                        {
                            _ClassType  = require(messageEntity.ClassName);
                        }
                        else
                        {
                            let arr = messageEntity.ClassName.split('.');
                            let path = '';
                            for (let i = 0; i < arr.length; i++) {
                                if(arr[i] != null && arr[i] !== undefined && arr[i] !== "")
                                    path = path + "/"+ arr[i] ;
                            }
                            _ClassType  = require(path);
                        }
                        let _ClassName = messageEntity.ClassName ;
                        let _instance = new _ClassType();
                        info = {className:_ClassName,type:_ClassType,instance:_instance };
                        this.registeredTypes.push(info) ;
                    }
                    let returnedObj = info.instance[messageEntity.MethodName](messageEntity.Params);
                    messageEntity.ReturnedData =returnedObj;
                }

            }
        }
        catch ( ex)
        {
            if(messageEntity==null)
                messageEntity = new CTCMessage()
            messageEntity.ErrorMsg = ex.message + " " + ex.stackTrace;
        }
        let resultString = JSON.stringify(messageEntity);
        return Buffer.from(resultString,this.encoding);
    }


    BYTES_CTS = 0;
    BYTES_STC = 1;
    BYTES_CTC = 2;
    BYTES_CTC_NoLoop = 3;
    FILE_BEGIN = 4;
    FILE_TRANSFER = 5;
    FILE_END = 6;
    DOWNLOAD_FILE = 7;
    REGISTER_SERVICE = 8;
    GET_REGISTERED_SERVICES = 9;
    UploadFileBegin = 10;
    UploadingFile = 11;
    UploadFileEnd = 12;
    DownloadFileRequest = 13;
    DownloadFileBegin = 14;
    DownloadingFile = 15;
    DownloadFileEnd = 16;
    NodeJSWebAPI = 17;
    //注册要监视的服务
    //主要是看服务是否有变化 （连接断开）
    RegisterSpyingService = 18;
    /// <summary>
    /// 当所监视的服务变化时（连接/断开）
    /// </summary>
    SpyingServiceChanged = 19;
    GET_CONFIG_FILE_CONTENT = 20;
    BootApp = 21;
}


module.exports = ServiceHost;
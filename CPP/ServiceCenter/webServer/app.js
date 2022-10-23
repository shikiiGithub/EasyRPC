
const http = require("http");
const Url = require('url')
const fs = require("fs");
const NodeJsClient = require('./NodeJsClient')
var httpIp = "127.0.0.1" ;
var httpPort = 5051 ;
let orderedActions = [] ;
const _APIProviderService = "APIProvider" ;
let _APIProviderServiceOnline = false ;
    /**
     * @type {NodeJsClient}
     */
let thisHost  = null ;
thisHost= new NodeJsClient() ;
let conf = thisHost.conf;
httpIp = conf.nodejs.ip ;
httpPort = conf.nodejs.port ;
orderedActions = conf.nodejs.orderedAction ;
thisHost.prepareEvent([_APIProviderService],'NodeJs WebApi Server',(serviceStatusInfo)=>{
    if(serviceStatusInfo[_APIProviderService] != null && serviceStatusInfo[_APIProviderService] != undefined)
    {
        let status = serviceStatusInfo[_APIProviderService] ;
        if(status !== undefined && status !== null)
            _APIProviderServiceOnline=status;
    }
},()=>{
    _APIProviderServiceOnline = false ;
}) ;
thisHost.Connect() ;
 
function onRequest(request, response) {
    let urlInfo = Url.parse(request.url,true) ;
    let args = urlInfo.query;
    let path = urlInfo.pathname ;
    let requestInfo = {
        url: request.url,
        method: request.method,
        path,
        args,//获取url地址栏的？后面名字为callback的参数
        headers: request.headers,
        content:''

    };
    let blockedFavicon = blockFavicon(request,response) ;
   
    if(!blockedFavicon)
    {
        //发送给ServiceCenter
        emitAction(request, requestInfo, response);
    }

}
/**
 * 发送结果到浏览器
 * @param statusCode {number}
 * @param response
 * @param result {String}
 */
function sendResult(statusCode, response, result) {
    let jsonHeader = "application/json;charset=utf-8";
    let plainTextHeader = "text/plain;charset=utf-8";
    let res = result.trim();
    let resultKindHeader = null;
    if (res.charAt(0) === '{')
        resultKindHeader = jsonHeader;
    else
        resultKindHeader = plainTextHeader;
    //将处理后的数据装载
    response.writeHead(statusCode, {
        "Access-Control-Allow-Origin": "*",
        "Access-Control-Allow-Headers": "*",
        "Content-Type": resultKindHeader
    });
    response.write(result);
    response.end();
}
function transferData2DotnetHost(reponse,str) {
    let message = thisHost.thisHost.getCTCMessage(_APIProviderService,'App','NodeJsWebAPIEntry',str)
    thisHost.thisHost.callService(_APIProviderService,message,
        (msg )=>{
            if(msg.ErrorMsg)
            {
                sendResult(500,reponse,msg.ErrorMsg)
            }
            else
            {
                let result = JSON.parse(msg.ReturnedData) ;
                sendResult(result.Code,reponse,msg.ReturnedData) ;
            }
        }
    ) ;

}
function emitAction(request, requestInfo, response) {
    let commonAction = ()=>{
         
        let simpleReturnMsgFromServiceCenter = (msg )=>{
            sendResult(200,response,msg) ;
          } ;
        
         let filteredAction = (actionName,command) =>{
            if(request.url.lastIndexOf(`/${actionName}`) !=  -1)
            {
              if(request.url.toLowerCase().lastIndexOf(`/bootapp`) != -1)
                {
                    let  json = JSON.stringify(requestInfo) ;
                    thisHost.thisHost.fromServiceCenter(simpleReturnMsgFromServiceCenter,command,json) ;
                }
                else 
                  thisHost.thisHost.fromServiceCenter(simpleReturnMsgFromServiceCenter,command) ;
              return true ;
            }
            else 
            return false ;
         } ;
         let  elementLen = orderedActions.length ;
         for (let i=0 ;i<elementLen;i++)
         {
            if(filteredAction(orderedActions[i].action,orderedActions[i].command))
               return  ;
         }
        if(thisHost.thisHost.connected &&  _APIProviderServiceOnline)
            transferData2DotnetHost(response,JSON.stringify(requestInfo))
        else
        {
            if(!thisHost.thisHost.connected)
            {
                sendResult(500,response,'未连接到 ServiceCenter ！')

            }
            else
            {
                if(!_APIProviderServiceOnline)
                    sendResult(500,response, _APIProviderService +' 不在线 ！')
            }

        }
    }
    if (request.method === 'POST') {
        let postData = '';
        //监听数据开始传输
        request.on('data', chunk => {
            postData += chunk.toString()
        })
        //监听数据传输结束
        request.on('end', () => {
            requestInfo.content  = postData ;
            commonAction() ;
        })
    }
    else
    {
        commonAction() ;
    }
}

http.createServer(onRequest).listen(httpPort,httpIp);
console.log("Server has started.");

function blockFavicon(request,response) {
    if(request.url==='/favicon.ico')
    {
        response.writeHead(404, {
            "Access-Control-Allow-Origin": "*",
            "Access-Control-Allow-Headers": "*",
        });
        response.write('');
        response.end();
        return  true;
    }
    else
        return false ;
}
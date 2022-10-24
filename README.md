# EasyRPC

I don't want to use something like Apache Thrift or Google gRPC,because they are so heavy , I only want a simple and light  RPC library ,so i've made it for me.

## Futures 

+ **Roles**

  It can be used as a RPC , Registering  Service and Discovery Other Services.

+ **Architecture**

  Base on TCP/IP protocol , one Service Center and many services .

+ **Easy** 

  Don't need to learn some concepts ,you just need recalling your service name , method ,Args that you need to pass they to the method you want to call.

+ **Support 4 Programming Language Client**

  + C++ ( including ServiceCenter and ServiceHost )

    it can compile to Windows Exe or Linux distributed package.

  + C# ( including ServiceCenter and ServiceHost )

  + Java (only ServiceHost )

  + NodeJs ( ServiceHost and  Light Http Web Server)

    It means you can set up your http Web Server to call a  service which write in other languages. 
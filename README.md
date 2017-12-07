# RabbitMQTest
利用RabbitMQ实现.net下的队列，主要实现取号功能<br/>
并且在开发完成后使用Jmeter做压力和负载测试<br/>
RabbitMQTest.Client-->RabbitMQTest.Server:简单的队列实现，客户端发送，服务端处理完成后不返回任何消息给客户端<br/>
RabbitMQTest.Web-->RabbitMQTest.RPCServer:实现RPC调用，客户端发送，服务端处理完成后将处理结果返回给客户端<br/>
RabbitMQTest.WCF-->RabbitMQTest.WCFServer:将队列集成到WCF服务<br/>

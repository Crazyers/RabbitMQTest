using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Util;

namespace RabbitMQTest.Server
{
    class Program
    {
        static void Main(string[] args)
        {
            IConnection conn = null;
            IModel channel = null;
            string exchangeName = "exchange.oa";
            string queueName = "queue.oa.contactlist.apply";
            string routingKey = "generateno";
            try
            {
                ConnectionFactory connFactory = new ConnectionFactory() { HostName = "10.7.249.21", VirtualHost = "/", UserName = "oa_admin", Password = "askey" };
                conn = connFactory.CreateConnection();
                channel = conn.CreateModel();
                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null);
                channel.QueueDeclare(queueName, true, false, false, null);
                channel.QueueBind(queueName, exchangeName, routingKey);
                channel.BasicQos(0, 1, false);//在一个工作者还在处理消息，并且没有响应消息之前，不要给他分发新的消息。相反，将这条新的消息发送给下一个不那么忙碌的工作者。
                var consumer = new QueueingBasicConsumer(channel);
                //将消费者与信道,队列相关联
                channel.BasicConsume(queue: queueName,
                         noAck: false,//true:开启消息自动确认功能,false:关闭消息自动确认功能==>消息确认后会自动将消息从队列中删除
                         consumer: consumer);
                while (true)
                {
                    try
                    {
                        var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                        var body = ea.Body;
                        var messageContent = Encoding.UTF8.GetString(body);
                        channel.BasicAck(ea.DeliveryTag, false);
                        Console.WriteLine(messageContent);
                    }
                    catch (Exception innerex)
                    {
                        continue;   
                    }
                }
            }
            catch (Exception ex)
            {
                
            }
            finally
            {
                if (channel != null && channel.IsOpen)
                {
                    channel.Close();
                }
                if (conn != null && conn.IsOpen)
                {
                    conn.Close();
                }
            }
        }
    }
}

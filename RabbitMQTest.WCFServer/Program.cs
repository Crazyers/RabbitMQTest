using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Util;
using System.Data;
using RabbitMQTest.Utils;
using RabbitMQTest.Model;

namespace RabbitMQTest.WCFServer
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
                channel.BasicQos(0, 1, false);
                var consumer = new QueueingBasicConsumer(channel);
                //将消费者与信道,队列相关联
                channel.BasicConsume(queue: queueName,
                         noAck: false,
                         consumer: consumer);
                while (true)
                {
                    var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                    var body = ea.Body;
                    var props = ea.BasicProperties;
                    var replyProps = channel.CreateBasicProperties();
                    replyProps.CorrelationId = props.CorrelationId;
                    replyProps.ReplyTo = props.ReplyTo;
                    ResponseEntity responseEntity = new ResponseEntity();
                    try
                    {
                        var message = Encoding.UTF8.GetString(body);
                        RequestEntity requestEntity= SerializeHelper.JsonDeserialize<RequestEntity>(message);
                        responseEntity= GenNoHelper.Insert4ContactListApply(requestEntity);
                    }
                    catch (Exception e)
                    {
                        responseEntity.HandleResult = false;
                        responseEntity.HandleMessage = e.Message;
                    }
                    finally
                    {
                        Console.WriteLine(string.Format("時間:{0},生成單號:{1},處理結果{2}", DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ffff"),responseEntity.No,responseEntity.HandleResult.ToString()));
                        string responseString = SerializeHelper.JsonSerialize(responseEntity);
                        var responseBytes = Encoding.UTF8.GetBytes(responseString);
                        channel.BasicPublish(exchange: "",
                                             routingKey: props.ReplyTo,
                                             basicProperties: replyProps,
                                             body: responseBytes);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag,
                                         multiple: false);
                    }
                }
            }
            catch (Exception ex)
            {
                //Mail通知维护人员处理程序异常
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

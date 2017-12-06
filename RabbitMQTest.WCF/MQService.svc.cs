using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using RabbitMQTest.Model;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Util;
using RabbitMQTest.Utils;

namespace RabbitMQTest.WCF
{
    public class MQService : IMQService
    {
        public ResponseEntity ContactListApply(RequestEntity request)
        {
            ResponseEntity response = new ResponseEntity();
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

                string replyQueueName = channel.QueueDeclare().QueueName;
                var consumer = new QueueingBasicConsumer(channel);
                channel.BasicConsume(queue: replyQueueName,
                                     noAck: false,
                                     consumer: consumer);

                var corrId = Guid.NewGuid().ToString();
                var props = channel.CreateBasicProperties();
                props.ReplyTo = replyQueueName;
                props.CorrelationId = corrId;
                props.DeliveryMode = 2;

                string requestBody= SerializeHelper.JsonSerialize(request);
                byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(requestBody);
                channel.BasicPublish(exchangeName, routingKey, props, messageBodyBytes);

                while (true)
                {
                    var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                    if (ea.BasicProperties.CorrelationId == corrId)
                    {
                        response = SerializeHelper.JsonDeserialize<ResponseEntity>(System.Text.Encoding.UTF8.GetString(ea.Body));
                        break;
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                return response;
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

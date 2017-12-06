using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Util;
using System.Text;
using RabbitMQTest.Model;
using RabbitMQTest.Utils;

namespace RabbitMQTest.Web
{
    public partial class WebForm1 : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnGenerateNo_Click(object sender, EventArgs e)
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

                byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ffff"));
                channel.BasicPublish(exchangeName, routingKey, props, messageBodyBytes);

                while (true)
                {
                    var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                    if (ea.BasicProperties.CorrelationId == corrId)
                    {
                        Response.Write(Encoding.UTF8.GetString(ea.Body));
                        break;
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

        protected void btnGenerateNoAndInsert_Click(object sender, EventArgs e)
        {
            RequestEntity request = new RequestEntity();
            request.GenNoType = "";
            request.SQLList = new List<string>();
            ResponseEntity response = GenerateNoByMQ(request);
            Response.Write(response.No);
        }

        public ResponseEntity GenerateNoByMQ(RequestEntity request)
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

                string requestBody = SerializeHelper.JsonSerialize(request);
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
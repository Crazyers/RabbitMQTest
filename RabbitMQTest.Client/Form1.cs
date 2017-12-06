using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Util;

namespace RabbitMQTest.Client
{
    /*
     * The core API interfaces and classes are defined in the RabbitMQ.Client namespace:
     * IModel: represents an AMQP 0-9-1 channel, and provides most of the operations (protocol methods).
     * IConnection: represents an AMQP 0-9-1 connection
     * ConnectionFactory: constructs IConnection instances
     * IBasicConsumer: represents a message consumer
     * DefaultBasicConsumer: commonly used base class for consumers
     */
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            IConnection conn=null;
            IModel channel=null;
            string exchangeName = "exchange.oa";
            string queueName = "queue.oa.contactlist.apply";
            string routingKey = "generateno";
            try
            {
                //Connecting to a Broker
                ConnectionFactory connFactory = new ConnectionFactory() { HostName = "10.7.249.21", VirtualHost = "/", UserName = "oa_admin", Password = "askey" };
                conn = connFactory.CreateConnection();

                //Use IConnection to open a channel
                channel = conn.CreateModel();

                //durable:持久化,将message保存到硬盘上,默认设定为true
                //auto-delete:自动删除,如果队列没有订阅的消费者,自动将message删除,默认要设定为false
                channel.ExchangeDeclare(exchangeName, ExchangeType.Direct, true, false, null);

                //durable:持久化,将message保存到硬盘上,默认设定为true
                //auto-delete:自动删除,如果队列没有订阅的消费者,自动将message删除,默认要设定为false
                //exclusive:排他队列,如果一个队列被声明为排他队列,该队列只对首次声明它的连接可见,并在连接断开是自动删除队列
                channel.QueueDeclare(queueName, true, false, false, null);
                channel.QueueBind(queueName, exchangeName, routingKey);
                byte[] messageBodyBytes = System.Text.Encoding.UTF8.GetBytes(this.txtMessage.Text+DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss ffff"));
                IBasicProperties prop = channel.CreateBasicProperties();
                //消息是否持久化
                prop.DeliveryMode = 2;
                channel.BasicPublish(exchangeName, routingKey, prop, messageBodyBytes);
                this.label3.Text = "Send Success";
            }
            catch(Exception ex)
            {
                this.label3.Text = ex.Message;
            }
            finally
            {
                if (channel!=null && channel.IsOpen)
                {
                    channel.Close();
                }
                if (conn!=null && conn.IsOpen)
                {
                    conn.Close();
                }
            }
        }
    }
}

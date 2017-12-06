using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Util;

namespace RabbitMQTest.RPCServer
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
                    var response = "";
                    try
                    {
                        var message = Encoding.UTF8.GetString(body);
                        string strNo = GenerateNo();
                        if (!string.IsNullOrEmpty(strNo))
                        {
                            OracleHelper.ExecuteNonQuery(OracleHelper.WEBDBConnection, "INSERT INTO tb_no_test(testno) VALUES ('" + strNo + "')");
                            response = strNo;
                        }
                        else
                        {
                            response = "Error";
                        }
                    }
                    catch (Exception e)
                    {
                        response = "Exception";
                    }
                    finally
                    {
                        Console.WriteLine(response);
                        var responseBytes = Encoding.UTF8.GetBytes(response);
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

        public static string GenerateNo()
        {
            string strYearMonth = DateTime.Now.ToString("yyyyMM");
            string strGetMaxID = "SELECT MAX(testno) id from tb_no_test ";//查询表中的字段   
            string maxID = Convert.ToString(OracleHelper.ExecuteScalar(OracleHelper.WEBDBConnection, CommandType.Text, strGetMaxID));
            string Result = "";
            if (maxID == "ThrowException")
            {
                Result = "";
            }
            else if (maxID == "")//没有最大编号   
            {
                Result = "S" + strYearMonth + "000000001";//S200902001   
            }
            else
            {
                //截取字符   
                string strFirstSix = maxID.Substring(1, 6);
                string strLastThree = maxID.Substring(7, 9);
                if (strYearMonth == strFirstSix)//截取的最大编号（20090225）是否和数据库服务器系统时间相等   
                {
                    string strNewFour = (Convert.ToInt32(strLastThree) + 1).ToString("000000000");//0000+1   
                    Result = "S" + strYearMonth + strNewFour;//CG2009020001   
                }
                else
                {
                    Result = "S" + strYearMonth + "000000001";
                }
            }
            return Result;
        }
    }
}

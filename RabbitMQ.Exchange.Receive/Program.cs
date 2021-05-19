using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RabbitMQ.Exchange.Receive
{
    class Program
    {
        static void Main(string[] args)
        {
            int random = new Random().Next(1, 1000);
            Console.WriteLine($"Start.QueueName:{random}");
            IConnectionFactory connFactory = new ConnectionFactory//创建连接工厂对象
            {
                HostName = "localhost",//IP地址
                Port = 5672,//端口号
                UserName = "guest",//用户账号
                Password = "guest"//用户密码
            };

            using (IConnection conn = connFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    String exchangeName = String.Empty;
                    if (args.Length > 0)
                        exchangeName = args[0];
                    else
                        exchangeName = "queue3";
                    channel.ExchangeDeclare(exchangeName, "fanout");

                    string queueName = exchangeName + "_" + random;
                    channel.QueueDeclare(queueName,true,false,false,null);

                    channel.QueueBind(queueName, exchangeName, "");
                    channel.BasicQos(0, 1, false);
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {

                        byte[] message = ea.Body.ToArray();//接收到的消息
                        Console.WriteLine("接收到信息为:" + Encoding.UTF8.GetString(message));
                        //返回消息确认
                        channel.BasicAck(ea.DeliveryTag, true);
                    };

                    channel.BasicConsume(queueName, false, consumer);
                    Console.ReadKey();
                }
            }
        }
    }
}

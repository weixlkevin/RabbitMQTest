using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RabbitMQ.RPC.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Serveice Start....");
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
                    //申明队列接收远程调用请求
                    channel.QueueDeclare(queue: "rpc_queue", durable: false,
                        exclusive: false, autoDelete: false, arguments: null);
                    var consumer = new EventingBasicConsumer(channel);
                    Console.WriteLine("[*] Waiting for message.");
                    //请求处理逻辑
                    consumer.Received += (model, ea) =>
                    {
                        var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                        int n = int.Parse(message);
                        Console.WriteLine($"Receive request of Fib({n})");
                        bool result = (n==1);
                        //从请求的参数中获取请求的唯一标识，在消息回传时同样绑定
                        var properties = ea.BasicProperties;
                        var replyProerties = channel.CreateBasicProperties();
                        replyProerties.CorrelationId = properties.CorrelationId;
                        //将远程调用结果发送到客户端监听的队列上
                        channel.BasicPublish(exchange: "", routingKey: properties.ReplyTo,
                            basicProperties: replyProerties, body: Encoding.UTF8.GetBytes(result.ToString()));
                        //手动发回消息确认
                        channel.BasicAck(ea.DeliveryTag, false);
                        Console.WriteLine($"Return result: {result}");
                    };
                    channel.BasicConsume(queue: "rpc_queue", autoAck: false, consumer: consumer);
                    Console.ReadKey();
                }
            }
            //申明唯一guid用来标识此次发送的远程调用请求
         
        }
    }
}

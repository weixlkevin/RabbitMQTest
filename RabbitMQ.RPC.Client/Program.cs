using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RabbitMQ.RPC.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Client Start....");
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
                    //申明唯一guid用来标识此次发送的远程调用请求
                    var correlationId = Guid.NewGuid().ToString();
                    //申明需要监听的回调队列
                    var replyQueue = channel.QueueDeclare().QueueName;
                    var properties = channel.CreateBasicProperties();
                    properties.ReplyTo = replyQueue;//指定回调队列
                    properties.CorrelationId = correlationId;//指定消息唯一标识
                    //string number = args.Length > 0 ? args[0] : "30";
                    //var body = Encoding.UTF8.GetBytes(number);
                    ////发布消息
                    //channel.BasicPublish(exchange: "", routingKey: "rpc_queue", basicProperties: properties, body: body);
                    //Console.WriteLine($"[*] Request fib({number})");
                    // //创建消费者用于处理消息回调（远程调用返回结果）
                    var callbackConsumer = new EventingBasicConsumer(channel);
                    channel.BasicConsume(queue: replyQueue, autoAck: true, consumer: callbackConsumer);
                    callbackConsumer.Received += (model, ea) =>
                    {
                        //仅当消息回调的ID与发送的ID一致时，说明远程调用结果正确返回。
                        if (ea.BasicProperties.CorrelationId == correlationId)
                        {
                            var responseMsg = $"Get Response: {Encoding.UTF8.GetString(ea.Body.ToArray())}";
                            Console.WriteLine($"[x]: {responseMsg}");
                        }
                    };
                    while (true)
                    {
                        Console.WriteLine("消息内容:");
                        string message = Console.ReadLine();
                        byte[] body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(exchange: "", routingKey: "rpc_queue", basicProperties: properties, body: body);
                        Console.WriteLine($"[*] Request fib({message})");
                    }

                }
            }
        }
    }
}

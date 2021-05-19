using RabbitMQ.Client;
using System;
using System.Text;

namespace RabbitMQ.Direct.Send
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Start");
            IConnectionFactory connectionFactory = new ConnectionFactory() { HostName = "localhost", Port = 5672, UserName = "guest", Password = "guest" };
            using (IConnection conn = connectionFactory.CreateConnection())
            {
                using (IModel channel = conn.CreateModel())
                {
                    string exchangeName = string.Empty;
                    if (args.Length > 0)
                    {
                        exchangeName = args[0];

                    }
                    else
                    {
                        exchangeName = "exdirect";
                    }
                    channel.ExchangeDeclare(exchange: exchangeName, type: "direct", durable: true, autoDelete: false);
                    while (true)
                    {
                        Console.WriteLine("消息内容:");
                        string message = Console.ReadLine();
                        byte[] body = Encoding.UTF8.GetBytes(message);
                        channel.BasicPublish(exchange: exchangeName, routingKey: "GPS", basicProperties: null, body: body);
                        Console.WriteLine($"成功发送消息:{message}");
                    }
                }
            }
        }
    }
}

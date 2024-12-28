using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading.Tasks;

class MessagePublisher
{
    public async Task PublishMessageAsync(string hostName, string queueName, string message)
    {
        var factory = new ConnectionFactory { HostName = hostName };
        using var connection = await factory.CreateConnectionAsync(); 
        using var channel = await connection.CreateChannelAsync(); 

        await channel.QueueDeclareAsync(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var body = Encoding.UTF8.GetBytes(message);

        await channel.BasicPublishAsync(
            exchange: "",
            routingKey: queueName,
            body: body
        );

        Console.WriteLine($"[x] Sent {message} to {queueName}");
    }
}
class Program
{
    static async Task Main(string[] args)
    {
        var publisher = new MessagePublisher();

        await publisher.PublishMessageAsync("localhost", "order-emails", "Order status changed to 'Shipped'");
        await publisher.PublishMessageAsync("localhost", "auth-emails", "Password reset link sent to user@example.com");

        Console.ReadLine();
    }
}

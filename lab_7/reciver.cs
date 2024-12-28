using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

class EmailService
{
    public async Task StartConsumingAsync(string hostName)
    {
        var factory = new ConnectionFactory { HostName = hostName };

        using var connection = await factory.CreateConnectionAsync();
        using var channel = await connection.CreateChannelAsync();


        await channel.QueueDeclareAsync("order-emails", durable: true, exclusive: false, autoDelete: false, arguments: null);
        await channel.QueueDeclareAsync("auth-emails", durable: true, exclusive: false, autoDelete: false, arguments: null);

        Console.WriteLine("[*] Waiting for messages.");

        await ConsumeQueueAsync(channel, "order-emails");
        await ConsumeQueueAsync(channel, "auth-emails");

        Console.WriteLine("Press [enter] to exit.");
        Console.ReadLine();
    }

    private async Task ConsumeQueueAsync(IChannel channel, string queueName)
    {
        var consumer = new AsyncEventingBasicConsumer(channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine($"[x] Received {message} from {queueName}");

            try
            {
                await ProcessEmailAsync(message);
                await channel.BasicAckAsync(ea.DeliveryTag, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[!] Error processing message: {ex.Message}");
                await channel.BasicNackAsync(ea.DeliveryTag, false, true); 
            }
        };

        await channel.BasicConsumeAsync(queueName, autoAck: false, consumer: consumer);
    }

    private async Task ProcessEmailAsync(string message)
    {
        await Task.Delay(500);
    }
}

class Program
{
    static async Task Main(string[] args)
    {
        var emailService = new EmailService();
        await emailService.StartConsumingAsync("localhost");

        Console.ReadLine();
    }
}

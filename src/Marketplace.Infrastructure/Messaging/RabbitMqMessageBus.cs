using System.Text;
using Marketplace.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Marketplace.Infrastructure.Messaging;

public sealed class RabbitMqMessageBus : IMessageBus
{
    private readonly IConfiguration _configuration;

    public RabbitMqMessageBus(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Publish(string queueName, string message)
    {
        var hostName = _configuration["RabbitMQ:HostName"] ?? "localhost";
        var port = int.TryParse(_configuration["RabbitMQ:Port"], out var parsedPort)
            ? parsedPort
            : 5672;
        var userName = _configuration["RabbitMQ:UserName"] ?? "guest";
        var password = _configuration["RabbitMQ:Password"] ?? "guest";

        var factory = new ConnectionFactory
        {
            HostName = hostName,
            Port = port,
            UserName = userName,
            Password = password
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: queueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(
            exchange: string.Empty,
            routingKey: queueName,
            basicProperties: null,
            body: body);
    }
}
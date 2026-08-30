using System.Text;
using Marketplace.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;

namespace Marketplace.Infrastructure.Messaging;

public class RabbitMqMessageBus : IMessageBus, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public RabbitMqMessageBus(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMQ:HostName"] ?? "localhost",
            Port = int.Parse(configuration["RabbitMQ:Port"] ?? "5672"),
            UserName = configuration["RabbitMQ:UserName"] ?? "guest",
            Password = configuration["RabbitMQ:Password"] ?? "guest"
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void Publish(string queueName, string message)
    {
        _channel.QueueDeclare(queue: queueName,
                             durable: false,
                             exclusive: false,
                             autoDelete: false,
                             arguments: null);

        var body = Encoding.UTF8.GetBytes(message);
        _channel.BasicPublish(exchange: "",
                             routingKey: queueName,
                             basicProperties: null,
                             body: body);
    }

    public void Dispose()
    {
        _channel.Close();
        _connection.Close();
    }
}

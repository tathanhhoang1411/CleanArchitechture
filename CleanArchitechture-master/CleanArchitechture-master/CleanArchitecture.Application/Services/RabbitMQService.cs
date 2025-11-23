using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Configuration;
using CleanArchitecture.Application.Interfaces;
namespace CleanArchitecture.Application.Services
{
    public class RabbitMQService: IRabbitMQService
    {
        private readonly ConnectionFactory _factory;
        private readonly string _queueName = "comment_events";

        public RabbitMQService(IConfiguration configuration)
        {
            _factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Host"],
                UserName = configuration["RabbitMQ:Username"],
                Password = configuration["RabbitMQ:Password"]
            };
        }

        public void Publish(string message)
        {
            using var connection = _factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false);
            var body = Encoding.UTF8.GetBytes(message);

            channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);
        }

        public void Subscribe(Action<string> onMessage)
        {
            var connection = _factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false);
            var consumer = new EventingBasicConsumer(channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                onMessage(message);
            };

            channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        }
    }
}
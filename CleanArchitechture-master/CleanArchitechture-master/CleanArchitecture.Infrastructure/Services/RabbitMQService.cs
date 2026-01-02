using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Microsoft.Extensions.Configuration;
using CleanArchitecture.Application.Interfaces;

namespace CleanArchitecture.Infrastructure.Services
{
    public class RabbitMQService : IRabbitMQService, IDisposable
    {
        private readonly ConnectionFactory _factory;
        private readonly string _queueName = "comment_events";
        private IConnection _connection;
        private IModel _channel;
        private readonly object _lock = new object();

        public RabbitMQService(IConfiguration configuration)
        {
            var vHost = configuration["RabbitMQ:VirtualHost"];
            if (string.IsNullOrEmpty(vHost)) vHost = configuration["RabbitMQ:Username"]; 

            _factory = new ConnectionFactory
            {
                HostName = configuration["RabbitMQ:Host"],
                UserName = configuration["RabbitMQ:Username"],
                Password = configuration["RabbitMQ:Password"],
                VirtualHost = vHost,
                Port = int.TryParse(configuration["RabbitMQ:Port"], out var port) ? port : 5672,
                RequestedConnectionTimeout = TimeSpan.FromSeconds(10), // Prevent long hangs
                AutomaticRecoveryEnabled = true
            };

            if (bool.TryParse(configuration["RabbitMQ:UseTls"], out var useTls) && useTls)
            {
                _factory.Ssl.Enabled = true;
                _factory.Ssl.ServerName = configuration["RabbitMQ:Host"];
            }
        }

        private void EnsureConnection()
        {
            if (_connection != null && _connection.IsOpen) return;

            lock (_lock)
            {
                if (_connection != null && _connection.IsOpen) return;

                try
                {
                    _connection?.Dispose();
                    _connection = _factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false);
                }
                catch (Exception ex)
                {
                    // Fallback or log if needed, but the caller will handle the exception
                    throw;
                }
            }
        }

        public void Publish(string message)
        {
            try
            {
                EnsureConnection();
                var body = System.Text.Encoding.UTF8.GetBytes(message);
                _channel.BasicPublish(exchange: "", routingKey: _queueName, basicProperties: null, body: body);
            }
            catch
            {
                // If publish fails, the next call will attempt to reconnect via EnsureConnection
            }
        }

        public void Subscribe(Action<string> onMessage)
        {
            EnsureConnection();
            var consumer = new EventingBasicConsumer(_channel);

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = System.Text.Encoding.UTF8.GetString(body);
                onMessage(message);
            };

            _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _connection?.Dispose();
        }
    }
}

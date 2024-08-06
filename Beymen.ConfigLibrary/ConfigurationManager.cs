using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using System.Text;
using System.Collections.Concurrent;

namespace Beymen.ConfigLibrary
{
    public class ConfigurationManager
    {
        private readonly string _serviceName;
        private readonly TimeSpan _checkInterval;
        private readonly Func<Task<Dictionary<string, string>>> _loadConfigurations;
        private ConcurrentDictionary<string, string> _configurations = new();
        private DateTime _lastUpdate;
        private bool _isActive;

        public ConfigurationManager(string serviceName, TimeSpan checkInterval, Func<Task<Dictionary<string, string>>> loadConfigurations, string rabbitMQConnectionString)
        {
            _serviceName = serviceName;
            _checkInterval = checkInterval;
            _loadConfigurations = loadConfigurations;
            _isActive = true;
            Task.Run(UpdateConfigurationsPeriodically);
            InitializeRabbitMQListener(rabbitMQConnectionString);
        }

        public T GetConfiguration<T>(string key)
        {
            if (_configurations.TryGetValue(key, out var value))
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            return default;
        }

        private async Task UpdateConfigurationsPeriodically()
        {
            while (_isActive)
            {
                await UpdateConfigurations();
                await Task.Delay(_checkInterval);
            }
        }

        private async Task UpdateConfigurations()
        {
            try
            {
                var newConfigurations = await _loadConfigurations();
                if (newConfigurations != null)
                {
                    foreach (var config in newConfigurations)
                    {
                        _configurations.AddOrUpdate(config.Key, config.Value, (key, oldValue) => config.Value);
                    }
                    _lastUpdate = DateTime.Now;
                }
            }
            catch
            {
                // Log error and continue with the last successful configurations
            }
        }

        private void InitializeRabbitMQListener(string rabbitMQConnectionString)
        {
            var factory = new ConnectionFactory() { Uri = new Uri(rabbitMQConnectionString) };
            var connection = factory.CreateConnection();
            var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "config_updates", type: ExchangeType.Fanout);

            var queueName = channel.QueueDeclare().QueueName;
            channel.QueueBind(queue: queueName, exchange: "config_updates", routingKey: "");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                if (message == _serviceName)
                {
                    await UpdateConfigurations();
                }
            };

            channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);
        }

        public void Stop()
        {
            _isActive = false;
        }
    }
}

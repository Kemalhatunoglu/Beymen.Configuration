using MongoDB.Driver;
using RabbitMQ.Client;
using System.Text;

namespace Beymen.ConfigStorage
{
    public class MongoConfigStorage
    {
        private readonly IMongoCollection<ConfigRecord> _collection;
        private readonly string _rabbitMQConnectionString;

        public MongoConfigStorage(string connectionString, string databaseName, string collectionName, string rabbitMQConnectionString)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<ConfigRecord>(collectionName);
            _rabbitMQConnectionString = rabbitMQConnectionString;
        }

        public async Task<Dictionary<string, string>> LoadConfigurationsAsync(string serviceName)
        {
            var filter = Builders<ConfigRecord>.Filter.Eq("ServiceName", serviceName) & Builders<ConfigRecord>.Filter.Eq("IsActive", true);
            var records = await _collection.Find(filter).ToListAsync();
            var configurations = new Dictionary<string, string>();

            foreach (var record in records)
            {
                configurations[record.Key] = record.Value;
            }

            return configurations;
        }

        public async Task UpdateConfigurationAsync(ConfigRecord record)
        {
            var filter = Builders<ConfigRecord>.Filter.Eq(r => r.ServiceName, record.ServiceName) & Builders<ConfigRecord>.Filter.Eq(r => r.Key, record.Key);
            var update = Builders<ConfigRecord>.Update.Set(r => r.Value, record.Value).Set(r => r.IsActive, record.IsActive);
            await _collection.UpdateOneAsync(filter, update);

            NotifyConfigurationChange(record.ServiceName);
        }

        private void NotifyConfigurationChange(string serviceName)
        {
            var factory = new ConnectionFactory() { Uri = new Uri(_rabbitMQConnectionString) };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(exchange: "config_updates", type: ExchangeType.Fanout);

            var message = Encoding.UTF8.GetBytes(serviceName);
            channel.BasicPublish(exchange: "config_updates", routingKey: "", basicProperties: null, body: message);
        }
    }
}

using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Beymen.ConfigStorage
{
    public class MongoConfigStorage
    {
        private readonly IMongoCollection<ConfigRecord> _collection;

        public MongoConfigStorage(string connectionString, string databaseName, string collectionName)
        {
            var client = new MongoClient(connectionString);
            var database = client.GetDatabase(databaseName);
            _collection = database.GetCollection<ConfigRecord>(collectionName);
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
    }
}

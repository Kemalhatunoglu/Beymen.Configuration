using Beymen.ConfigStorage;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

namespace Beymen.Configuration.Test
{
    [TestFixture]
    public class MongoConfigStorageTests
    {
        private IMongoDatabase _database;
        private MongoConfigStorage _configStorage;
        private Beymen.ConfigLibrary.ConfigurationManager _configManager;
        private IMongoCollection<ConfigRecord> _configCollection;
        private IConfiguration _configuration;
        private readonly string _testDatabaseName = "TestConfigDB";
        private readonly string _testCollectionName = "TestConfigs";

        [SetUp]
        public void Setup()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Test projesinin çalışma dizini
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = configurationBuilder.Build();

            var mongoClient = new MongoClient(_configuration["MongoConnectionString"]);

            _database = mongoClient.GetDatabase(_testDatabaseName);

            _configCollection = _database.GetCollection<ConfigRecord>(_testCollectionName);

            _configCollection.DeleteMany(FilterDefinition<ConfigRecord>.Empty);

            _configStorage = new MongoConfigStorage(_configuration["MongoConnectionString"], _testDatabaseName, _testCollectionName, _configuration["RabbitMQConnectionString"]);

            _configManager = new Beymen.ConfigLibrary.ConfigurationManager(
                _configuration["ServiceName"],
                TimeSpan.FromSeconds(int.Parse(_configuration["ConfigCheckIntervalSeconds"])),
                 () => Task.FromResult(new Dictionary<string, string>
                {
                { "TestKey1", _configuration["TestKey1"] },
                { "TestKey2", _configuration["TestKey2"] },
                { "TestKey3", _configuration["TestKey3"] }
                }),
                _configuration["RabbitMQConnectionString"]
            );
        }

        [Test]
        public async Task LoadConfigurationsAsync_ReturnsCorrectValues()
        {
            var collection = _database.GetCollection<ConfigRecord>("TestConfigs");

            await collection.InsertManyAsync(new List<ConfigRecord>
            {
                new() {Id = Guid.NewGuid().ToString(), ServiceName = "TEST-SERVICE", Key = "TestKey1", Value = "TestValue1", IsActive = true },
                new() {Id = Guid.NewGuid().ToString(), ServiceName = "TEST-SERVICE", Key = "TestKey2", Value = "42", IsActive = true },
                new() {Id = Guid.NewGuid().ToString(), ServiceName = "TEST-SERVICE", Key = "TestKey3", Value = "true", IsActive = false }
            });

            var configs = await _configStorage.LoadConfigurationsAsync("TEST-SERVICE");

            Assert.AreEqual("TestValue1", configs["TestKey1"]);
            Assert.AreEqual("42", configs["TestKey2"]);
            Assert.IsFalse(configs.ContainsKey("TestKey3"));
        }

        [Test]
        public async Task UpdateConfigurationAsync_UpdatesValueCorrectly()
        {
            var collection = _database.GetCollection<ConfigRecord>("TestConfigs");

            var record = new ConfigRecord { Id = Guid.NewGuid().ToString(), ServiceName = "TEST-SERVICE", Key = "TestKey1", Value = "InitialValue", IsActive = true };
            await collection.InsertOneAsync(record);

            record.Value = "UpdatedValue";
            await _configStorage.UpdateConfigurationAsync(record);

            var configs = await _configStorage.LoadConfigurationsAsync("TEST-SERVICE");

            Assert.AreEqual("UpdatedValue", configs["TestKey1"]);
        }

        [TearDown]
        public void TearDown()
        {
            _database.DropCollection("TestConfigs");
        }
    }
}

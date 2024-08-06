using Microsoft.Extensions.Configuration;

namespace Beymen.Configuration.Test
{
    [TestFixture]
    public class ConfigurationManagerTests
    {
        private Beymen.ConfigLibrary.ConfigurationManager _configManager;
        private IConfiguration _configuration;

        [SetUp]
        public void Setup()
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()) // Test projesinin çalışma dizini
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            _configuration = configurationBuilder.Build();

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
        public void GetConfiguration_ReturnsCorrectValue()
        {
            var value = _configManager.GetConfiguration<string>("TestKey1");
            Assert.AreEqual("TestValue1", value);
        }

        [Test]
        public void GetConfiguration_ReturnsCorrectIntegerValue()
        {
            var value = _configManager.GetConfiguration<int>("TestKey2");
            Assert.AreEqual(42, value);
        }

        [Test]
        public void GetConfiguration_ReturnsCorrectBooleanValue()
        {
            var value = _configManager.GetConfiguration<bool>("TestKey3");
            Assert.AreEqual(true, value);
        }

        [Test]
        public void GetConfiguration_ReturnsDefaultForUnknownKey()
        {
            var value = _configManager.GetConfiguration<string>("UnknownKey");
            Assert.IsNull(value);
        }

        [TearDown]
        public void TearDown()
        {
            _configManager.Stop();
        }
    }
}
namespace Beymen.Configuration
{
    public class ConfigurationManager
    {
        private readonly string _serviceName;
        private readonly TimeSpan _checkInterval;
        private readonly Func<Task<Dictionary<string, string>>> _loadConfigurations;
        private Dictionary<string, string> _configurations = new();
        private DateTime _lastUpdate;
        private bool _isActive;

        public ConfigurationManager(string serviceName, TimeSpan checkInterval, Func<Task<Dictionary<string, string>>> loadConfigurations)
        {
            _serviceName = serviceName;
            _checkInterval = checkInterval;
            _loadConfigurations = loadConfigurations;
            _isActive = true;
            Task.Run(UpdateConfigurationsPeriodically);
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
                    _configurations = newConfigurations;
                    _lastUpdate = DateTime.Now;
                }
            }
            catch
            {
                // Log error and continue with the last successful configurations
            }
        }

        public void Stop()
        {
            _isActive = false;
        }
    }
}

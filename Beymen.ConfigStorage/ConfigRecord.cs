namespace Beymen.ConfigStorage
{
    public class ConfigRecord
    {
        public string Id { get; set; }
        public string ServiceName { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public bool IsActive { get; set; }
    }
}

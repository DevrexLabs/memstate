namespace Memstate.Models.KeyValue
{
    public class Set<T> : Command<KeyValueStore<T>, int>
    {
        public Set(string key, T value, int? expectedVersion = null)
        {
            Key = key;
            Value = value;
            ExpectedVersion = expectedVersion;
        }
        
        public string Key { get; }
        
        public T Value { get; }
        
        public int? ExpectedVersion { get; }
        
        public override int Execute(KeyValueStore<T> model)
        {
            return model.Set(Key, Value, ExpectedVersion);
        }
    }
}
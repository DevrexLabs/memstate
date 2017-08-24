namespace Memstate.Models.KeyValue
{
    public class Remove<T> : Command<KeyValueStore<T>>
    {
        public Remove(string key, int? expectedVersion)
        {
            Key = key;
            ExpectedVersion = expectedVersion;
        }
        
        public string Key { get; }
        
        public int? ExpectedVersion { get; }

        public override void Execute(KeyValueStore<T> model)
        {
            model.Remove(Key, ExpectedVersion);
        }
    }
}
namespace Memstate.Models.KeyValue
{
    public class Get<T> : Query<KeyValueStore<T>, KeyValueStore<T>.Node>
    {
        public Get(string key)
        {
            Key = key;
        }
        
        public string Key { get; }
        
        public override KeyValueStore<T>.Node Execute(KeyValueStore<T> model)
        {
            return model.Get(Key);
        }
    }
}
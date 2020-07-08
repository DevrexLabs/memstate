namespace Memstate.Models
{
    public interface IKeyValueStore<T>
    {
        KeyValueStore<T>.Node Get(string key);

        void Remove(string key, int? expectedVersion = null);

        [Command]
        int Set(string key, T value, int? expectedVersion = null);

        int Count();
    }
}
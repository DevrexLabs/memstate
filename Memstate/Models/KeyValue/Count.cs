namespace Memstate.Models.KeyValue
{
    /// <summary>
    /// Return the number of keys in the store
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Count<T> : Query<KeyValueStore<T>, int>
    {
        public override int Execute(KeyValueStore<T> db)
        {
            return db.Count();
        }
    }
}
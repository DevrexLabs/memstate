namespace Memstate.Models.KeyValue
{
    /// <summary>
    /// Return the number of keys in the store
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Count<T> : Query<IKeyValueStore<T>, int>
    {
        public override int Execute(IKeyValueStore<T> db)
        {
            return db.Count();
        }
    }
}
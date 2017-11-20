namespace Memstate.Models.Relational
{
    public static class RelationalExtensions
    {
        public static void Execute(this RelationalModel model, Batch batch)
        {
            model.DoExecute(batch);

            batch.Inserts.ForEach(x => x.Version++);
            batch.Updates.ForEach(x => x.Version++);
        }

        public static void Update(this RelationalModel model, params IEntity[] entities)
        {
            model.Update(entities);

            foreach (var entity in entities)
            {
                entity.Version++;
            }
        }

        public static void Insert(this RelationalModel model, params IEntity[] entities)
        {
            model.Insert(entities);

            foreach (var entity in entities)
            {
                entity.Version++;
            }
        }
    }
}
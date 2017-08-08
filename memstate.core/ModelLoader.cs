namespace Memstate.Core
{
    public class ModelLoader
    {
        public ModelLoader()
        {
            LastRecordNumber = -1;
        }

        public long LastRecordNumber { get; private set; }

        public TModel Load<TModel>(IJournalReader reader) where TModel:new()
        {
            TModel model = new TModel();
            foreach (var journalRecord in reader.GetRecords())
            {
                try
                {
                    journalRecord.Command.ExecuteImpl(model);
                    LastRecordNumber = journalRecord.RecordNumber;
                }
                catch {}
            }
            return model;
        }
    }
}
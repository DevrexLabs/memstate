using System;

namespace Memstate
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

            return Load(reader, model);
        }

        public TModel Load<TModel>(IJournalReader reader, TModel model)
        {
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

        public TModel Load<TModel>(IJournalReader reader, Func<TModel> constructor)
        {
            return Load(reader, constructor.Invoke());
        }
    }
}
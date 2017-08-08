using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Memstate.Core
{
    public class ModelLoader
    {
        public ModelLoader()
        {
            
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

    public interface IJournalReader : IDisposable
    {
        IEnumerable<JournalRecord> GetRecords();
    }

    public class FileJournalReader : IJournalReader

    {
    private readonly FileStream _journalStream;
    private readonly ISerializer _serializer;

    public FileJournalReader(String fileName, ISerializer serializer)
    {
        _journalStream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        _serializer = serializer;
    }

    public void Dispose()
    {
        _journalStream.Dispose();
    }

    public IEnumerable<JournalRecord> GetRecords()
    {
        foreach (var records in _serializer.ReadObjects<JournalRecord[]>(_journalStream))
        {
            foreach (var record in records)
            {
                yield return record;
            }
        }
    }
    }
}
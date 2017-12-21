using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Memstate
{
    public class FileJournalReader : IJournalReader
    {
        private readonly string _fileName;

        private readonly MemstateSettings _settings;

        private readonly ISerializer _serializer;

        public FileJournalReader(string fileName, MemstateSettings settings)
        {
            _fileName = fileName;
            _settings = settings;
            _serializer = settings.CreateSerializer();
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        public IEnumerable<JournalRecord> GetRecords(long fromRecord = 0)
        {
            foreach (var stream in Files())
            {
                using (stream)
                {
                    foreach (var records in _serializer.ReadObjects<JournalRecord[]>(stream))
                    {
                        foreach (var record in records)
                        {
                            if (record.RecordNumber >= fromRecord)
                            {
                                yield return record;
                            }
                        }
                    }
                }
            }
        }

        private IEnumerable<Stream> Files()
        {
            if (_settings.FileSystem.Exists(_fileName))
            {
                yield return _settings.FileSystem.OpenRead(_fileName);
            }
            else
            {
                var index = 0;

                while (_settings.FileSystem.Exists(string.Format(_fileName, index)))
                {
                    yield return _settings.FileSystem.OpenRead(string.Format(_fileName, index++));
                }
            }
        }
    }
}
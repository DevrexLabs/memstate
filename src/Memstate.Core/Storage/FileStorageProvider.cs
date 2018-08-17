using System;
using System.Threading.Tasks;
using Memstate.Configuration;

namespace Memstate
{
    public class FileStorageProvider : StorageProvider
    {
        private readonly FileStorageSettings _fileStorageSettings;
        private readonly EngineSettings _settings;
        private FileJournalWriter _currentWriter;
        private readonly IFileSystem _fileSystem;


        public FileStorageProvider()
        {
            var cfg = Config.Current;
            _settings = cfg.Resolve<EngineSettings>();
            _fileStorageSettings = cfg.Resolve<FileStorageSettings>();
            _fileSystem = cfg.FileSystem;
        }

        public override IJournalReader CreateJournalReader()
        {
            var fileName = _fileStorageSettings.FileName;

            if (!_fileSystem.Exists(fileName))
            {
                return new NullJournalReader();
            }

            return new FileJournalReader(fileName);
        }

        public override IJournalWriter CreateJournalWriter(long nextRecordNumber)
        {
            var fileName = _fileStorageSettings.FileName;
            _currentWriter = new FileJournalWriter(fileName, nextRecordNumber);
            return _currentWriter;
        }

        public override IJournalSubscriptionSource CreateJournalSubscriptionSource()
        {
            if (_currentWriter == null)
            {
                throw new InvalidOperationException("Cannot create subscriptionsource");
            }

            return new FileJournalSubscriptionSource(_currentWriter);
        }

        public Task DisposeAsync() => Task.CompletedTask;

        public override bool SupportsCatchupSubscriptions() => false;
    }
}
namespace Memstate
{
    using System;
    using System.IO;
    using System.Threading.Tasks;

    public class FileStorageProvider : StorageProvider
    {
        private readonly FileStorageSettings _fileStorageSettings;
        private readonly MemstateSettings _settings;
        private FileJournalWriter _currentWriter;

        public FileStorageProvider(MemstateSettings settings)
        {
            _settings = settings;
            _fileStorageSettings = new FileStorageSettings(settings);
        }

        public override IJournalReader CreateJournalReader()
        {
            var fileName = _fileStorageSettings.FileName;

            if (!File.Exists(fileName))
            {
                return new NullJournalReader();
            }

            var serializer = _settings.CreateSerializer();
            return new FileJournalReader(fileName, serializer);
        }

        public override IJournalWriter CreateJournalWriter(long nextRecordNumber)
        {
            var fileName = _fileStorageSettings.FileName;
            _currentWriter = new FileJournalWriter(_settings, fileName, nextRecordNumber);
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

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }
    }
}
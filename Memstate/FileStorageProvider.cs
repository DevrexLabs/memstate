namespace Memstate
{
    using System;
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

            if (!_settings.FileSystem.Exists(fileName))
            {
                return new NullJournalReader();
            }

            return new FileJournalReader(fileName, _settings);
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

        public Task DisposeAsync() => Task.CompletedTask;

        public override bool SupportsCatchupSubscriptions() => false;
    }
}
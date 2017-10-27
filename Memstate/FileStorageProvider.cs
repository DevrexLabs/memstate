namespace Memstate
{
    using System;

    public class FileStorageProvider : StorageProvider
    {
        private readonly FileStorageSettings _fileStorageSettings;
        private readonly Settings _settings;
        private FileJournalWriter _currentWriter;

        public FileStorageProvider(Settings settings)
            : base(settings)
        {
            _settings = settings;
            _fileStorageSettings = new FileStorageSettings();
            settings.Bind(_fileStorageSettings, "StorageProviders:FileStorage");
        }

        public override IJournalReader CreateJournalReader()
        {
            var fileName = _fileStorageSettings.FileName;
            var serializer = _settings.GetSerializer();
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

        public override void Dispose()
        {
        }
    }
}
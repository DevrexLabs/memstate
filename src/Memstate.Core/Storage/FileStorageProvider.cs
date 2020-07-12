using System;
using System.Threading.Tasks;
using Memstate.Configuration;

namespace Memstate
{
    internal class FileStorageProvider : IStorageProvider
    {
        private FileJournalWriter _currentWriter;

        private readonly Config _config;
        private readonly string _fileName;


        public FileStorageProvider(Config config)
        {
            _config = config;
            var settings = config.GetSettings<EngineSettings>();
            var fileStorageSettings = config.GetSettings<FileStorageSettings>();
            _fileName = (fileStorageSettings.FileNameWithoutSuffix ?? settings.StreamName) +
                        fileStorageSettings.FileNameSuffix;
        }

        public Task Provision() => Task.CompletedTask;

        public IJournalReader CreateJournalReader()
        {
            if (_currentWriter is null) throw new InvalidOperationException("Create a writer before creating a reader");
            return new FileJournalReader(_config, _fileName, _currentWriter);
        }

        public IJournalWriter CreateJournalWriter()
        {
            if (_currentWriter != null) throw new InvalidOperationException("Can only create one writer per FileStorageProvider");
            _currentWriter = new FileJournalWriter(_config, _fileName);
            return _currentWriter;
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
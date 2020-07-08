using System;
using System.Threading.Tasks;
using Memstate.Configuration;

namespace Memstate
{
    public class FileStorageProvider : IStorageProvider
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
            return new FileJournalReader(_config, _fileName, _currentWriter);
        }

        public IJournalWriter CreateJournalWriter()
        {
            //todo: figure out a way to initialize
            var nextRecordNumber = 0;
            _currentWriter = new FileJournalWriter(_config, _fileName, nextRecordNumber);
            return _currentWriter;
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
using System;
using System.Threading.Tasks;
using Memstate.Configuration;

namespace Memstate
{
    public class FileStorageProvider : IStorageProvider
    {
        private readonly FileStorageSettings _fileStorageSettings;
        private readonly EngineSettings _settings;
        private FileJournalWriter _currentWriter;
        private readonly IFileSystem _fileSystem;


        public FileStorageProvider()
        {
            var cfg = Config.Current;
            _settings = cfg.GetSettings<EngineSettings>();
            _fileStorageSettings = cfg.GetSettings<FileStorageSettings>();
            _fileSystem = cfg.FileSystem;
        }

        public Task Provision()
        {
            return Task.CompletedTask;
        }

        public IJournalReader CreateJournalReader()
        {
            var fileName = _fileStorageSettings.FileName;
            return new FileJournalReader(fileName);
        }

        public IJournalWriter CreateJournalWriter()
        {
            //todo: figure out a way to initialize
            var nextRecordNumber = 0;
            var fileName = _fileStorageSettings.FileName;
            _currentWriter = new FileJournalWriter(fileName, nextRecordNumber);
            return _currentWriter;
        }

        public Task DisposeAsync() => Task.CompletedTask;
    }
}
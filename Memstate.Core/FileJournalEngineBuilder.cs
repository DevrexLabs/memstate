using Microsoft.Extensions.Configuration;

namespace Memstate.Core
{
    public class FileJournalEngineBuilder : IEngineBuilder
    {
        private readonly string _journalFile;
        private readonly Config _config;
        public FileJournalEngineBuilder(Config config, string journalFile)
        {
            _journalFile = journalFile;
            _config = config;
        }
        public Engine<TModel> Load<TModel>() where TModel:class,new()
        {
            ISerializer serializer = _config.GetSerializer();
            var reader = new FileJournalReader(_journalFile, serializer);
            
            var loader = new ModelLoader();
            var model = loader.Load<TModel>(reader);
            var nextRecordNumber = loader.LastRecordNumber + 1;
            var writer = new FileJournalWriter(_config, serializer, _journalFile, nextRecordNumber);
            var subscriptionSource = new FileJournalSubscriptionSource(writer);
            return new Engine<TModel>(_config, model, subscriptionSource, writer, nextRecordNumber);
        }
    }

    public interface IEngineBuilder
    {
        Engine<T> Load<T>() where T : class, new();
    }
}
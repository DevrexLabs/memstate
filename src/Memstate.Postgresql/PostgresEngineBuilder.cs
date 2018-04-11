namespace Memstate.Postgresql
{
    public class PostgresEngineBuilder : IEngineBuilder
    {
        private readonly Config _config;

        private readonly PostgresJournalReader _commandStore;

        public PostgresEngineBuilder(Config config)
            : this(config, new PostgresJournalReader(config))
        {
        }

        public PostgresEngineBuilder(Config config, PostgresJournalReader commandStore)
        {
            _config = config;
            _commandStore = commandStore;
        }
        
        public Engine<T> Build<T>() where T : class, new()
        {
            return Build(new T());
        }

        public Engine<T> Build<T>(T initialModel) where T : class
        {
            var loader = new ModelLoader();

            var model = loader.Load(_commandStore, initialModel);

            // TODO: Figure out if next record should be 0 or calculated.
            var engine = new Engine<T>(_config, model, _commandStore, _commandStore, 0);

            return engine;
        }
    }
}
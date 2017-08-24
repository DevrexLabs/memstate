namespace Memstate
{
    public class InMemoryEngineBuilder : IEngineBuilder
    {
        private readonly Config _config;
        private readonly InMemoryCommandStore _commandStore;

        public InMemoryEngineBuilder(Config config)
            : this(config, new InMemoryCommandStore(config))
        {
        }

        public InMemoryEngineBuilder(Config config, InMemoryCommandStore commandStore)
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
            
            var engine = new Engine<T>(_config, model, _commandStore, _commandStore, 0);

            return engine;
        }
    }
}
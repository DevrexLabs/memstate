﻿using Memstate.Configuration;
using SqlStreamStore;
using SqlStreamStore.Streams;

namespace Memstate.SqlStreamStore
{
    public class SqlStreamStoreProvider: StorageProvider
    {
        private readonly StreamId _streamId;
        private readonly ISerializer _serializer;
        private readonly IStreamStore _streamStore;

        public SqlStreamStoreProvider() : this(null)
        {
            
        }
        public SqlStreamStoreProvider(IStreamStore streamStore)
        {
            Config config = Config.Current;
            _serializer = config.CreateSerializer();
            var settings = config.GetSettings<EngineSettings>();
            _streamId = new StreamId(settings.StreamName);

            if (streamStore == null)
            {
                if (!config.Container.TryResolve(out streamStore))
                    streamStore = new InMemoryStreamStore();
            }

            _streamStore = streamStore;
        }

        public override IJournalReader CreateJournalReader()
        {
            return new SqlSteamStoreSubscriptionJournalReader(
                _streamStore,
                _streamId,
                _serializer);

            //return new SqlStreamSourceJournalReader(
            //    _streamStore,
            //    _streamId,
            //    _serializer);
        }

        public override IJournalWriter CreateJournalWriter(long nextRecordNumber)
        {
            return new SqlStreamStoreJournalWriter(_streamStore, _streamId, _serializer);
        }

        public override IJournalSubscriptionSource CreateJournalSubscriptionSource()
        {
            return new SqlStreamStoreSubscriptionSource(_streamStore, _streamId, _serializer);
        }
    }
}
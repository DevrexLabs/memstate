using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Memstate.Core
{
    public class FileCommandStore : IHandle<Command>, ICommandSubscriptionSource
    {
        private readonly Dictionary<Guid, Subscription> _subscriptions;
        private readonly Batcher<Command> _commandBatcher;
        private long _nextRecord;
        private readonly FileStream _journalStream;
        private readonly ISerializer _serializer;
        private readonly FileRecordLayout _fileLayout;

        public FileCommandStore(String fileName, ISerializer serializer, LayoutOptions layoutOptions = LayoutOptions.None)
        {
            _journalStream = File.Open(fileName, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            _commandBatcher = new Batcher<Command>(OnCommandBatch, 100);
            _subscriptions = new Dictionary<Guid, Subscription>();
            _nextRecord = _journalStream.Length == 0 ? 1 : 1 + ReadLastRecordNumber();
            _serializer = serializer;
            _fileLayout = new FileRecordLayout(layoutOptions);
        }

        /// <summary>
        /// Last 8 bytes of file is the record number of the last record
        /// </summary>
        /// <returns></returns>
        private long ReadLastRecordNumber()
        {
            _journalStream.Position = _journalStream.Length - 8;
            return new BinaryReader(_journalStream).ReadInt64();
        }

        public void Dispose()
        {
            _commandBatcher.Dispose();
            _journalStream.Dispose();
        }

        private JournalRecord ToJournalRecord(Command command)
        {
            return new JournalRecord(_nextRecord++, DateTime.Now, command);
        }

        private void OnCommandBatch(IEnumerable<Command> commands)
        {
            
            var records = commands.Select(ToJournalRecord).ToArray();
            var bytes = _serializer.Serialize(records);
            lock (_journalStream)
            {
                var writer = new BinaryWriter(_journalStream);
                _fileLayout.Write(bytes, writer);

                //write the record number of the last record, and backup it will be overwritten by the next batch
                //so only the last 8 bytes of the file will have a record number
                writer.Write(records.Last().RecordNumber);
                writer.Flush();
                _journalStream.Flush(true);
                SeekToAppendPosition();
            }
            foreach (var record in records)
            {
                foreach (var sub in _subscriptions.Values)
                {
                    sub.Handle(record);
                }
            }
        }

        public void Handle(Command command)
        {
            _commandBatcher.Append(command);
        }

        private void RemoveSubscription(Subscription subscription)
        {
            lock (_subscriptions)
            {
                _subscriptions.Remove(subscription.Id);
            }
        }

        private IEnumerable<JournalRecord> GetRecords()
        {
            lock (_journalStream)
            {
                _journalStream.Position = 0;
                var reader = new BinaryReader(_journalStream);
                long numRecords = _nextRecord - 1;
                long lastRecordRead = 0;
                while (lastRecordRead < numRecords)
                {
                    var bytes = _fileLayout.Read(reader);
                    var records = (JournalRecord[])_serializer.Deserialize(bytes);
                    foreach (var record in records)
                    {
                        lastRecordRead = record.RecordNumber;
                        yield return record;
                    }
                }
                SeekToAppendPosition();
            }
        }

        private void SeekToAppendPosition()
        {
            _journalStream.Position = Math.Max(_journalStream.Length-8, 0);
        }

        public ICommandSubscription Subscribe(long from, Action<JournalRecord> handler)
        {
            var subscription = new Subscription(handler, from, RemoveSubscription);
            lock (_subscriptions) _subscriptions.Add(subscription.Id, subscription);
            foreach (var record in GetRecords().SkipWhile(record => record.RecordNumber < from))
            {
                subscription.Handle(record);
            }
            return subscription;
        }
    }
}
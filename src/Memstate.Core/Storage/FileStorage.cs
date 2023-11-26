using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Memstate;

public class FileStorage : IStorage
{
    private readonly IFileSystem _fileSystem;
    private readonly string _journalFile;
    private readonly ISerializer _serializer;
    private const int RecordsPerChunk = 64;

    private long _nextRecordNumber = 1;

    private Stream _stream;
    private StreamWriter _streamWriter;
    public FileStorage(IFileSystem fileSystem, String journalFile, ISerializer serializer)
    {
        _fileSystem = fileSystem;
        _journalFile = journalFile;
        _serializer = serializer;
    }
    public Task DisposeAsync()
    {
        _stream.Dispose();
        return Task.CompletedTask;
    }

    public async IAsyncEnumerable<JournalRecord[]> ReadRecords(long from = 1)
    {
        if (!_fileSystem.Exists(_journalFile)) yield break;
        _stream = _fileSystem.OpenRead(_journalFile);
        var reader = new StreamReader(_stream);
        var records = new List<JournalRecord>();
        long lastRecordNumberRead = 0;
        while (true)
        {
            var line = await reader.ReadLineAsync();
            if (line is null) break;
            var record = (JournalRecord)_serializer.FromString(line);
            lastRecordNumberRead = record.RecordNumber;
            if (record.RecordNumber < from) continue;
            records.Add(record);
            if (records.Count == RecordsPerChunk)
            {
                yield return records.ToArray();
                records.Clear();
            }
        }

        if (records.Any()) yield return records.ToArray();

        _nextRecordNumber = lastRecordNumberRead + 1;
    }

    public async Task<JournalRecord> Append(Command command)
    {
        if (_streamWriter is null)
        {
            _stream = _fileSystem.OpenAppend(_journalFile);
            _streamWriter = new StreamWriter(_stream);
            
        }
        var record = new JournalRecord(_nextRecordNumber++, DateTimeOffset.Now, command);
        _serializer.ToString(record);
        await _streamWriter.WriteLineAsync(_serializer.ToString(record));
        return record;
    }
}
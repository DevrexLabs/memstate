using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Memstate.Logging;

namespace Memstate;

public class Engine<TModel> where TModel : class
{
    private readonly ILog _logger;

    private readonly Kernel _kernel;

    private readonly EngineSettings _settings;
        

    private readonly IEngineMetrics _metrics;

    private volatile bool _stopped;

    /// <summary>
    /// Last record number applied to the model, 0 if initial model
    /// Numbering starts from 1
    /// </summary>
    private long _lastRecordNumber;

    private DateTimeOffset _lastCommandExecuted = DateTimeOffset.MinValue;

    private readonly IStorage _storage;
    public event CommandExecuted CommandExecuted = delegate { };

    public Engine(IStorage storage, EngineSettings settings, TModel model, long recordNumber)
    {
        _lastRecordNumber = recordNumber;
        _logger = LogProvider.GetCurrentClassLogger();
        _kernel = new Kernel(settings, model);
        _settings = settings;
        _metrics = Metrics.Provider.GetEngineMetrics();
    }

    public long LastRecordNumber => Interlocked.Read(ref _lastRecordNumber);

    public async Task<TResult> Execute<TResult>(Command<TModel, TResult> command)
    {
        return (TResult) await ExecuteCommand(command);
    }

    public Task Execute(Command<TModel> command)
    {
        return ExecuteCommand(command);
    }

    public async Task<TResult> Execute<TResult>(Query<TModel, TResult> query)
    {
        return (TResult) await ExecuteQuery(query);
    }

    public Task DisposeAsync()
    {
        return _storage.DisposeAsync();
    }
        
    internal async Task<object> ExecuteQuery(Query query)
    {

        if (ShouldRefresh()) await Refresh();

        using (_metrics.MeasureQueryExecution())
        {
            try
            {
                var result = _kernel.Execute(query);
                _metrics.QueryExecuted();
                return result;
            }
            catch (Exception)
            {
                _metrics.QueryFailed();
                throw;
            }
        }
    }

    /// <summary>
    /// Do we need to check for newer commands from storage
    /// before executing a query? 
    /// </summary>
    private bool ShouldRefresh()
    {
        //Single engine running? -> no, there
        //time elapsed since last command exceeds limit? -> yes
        // user explicitly asked for it? -> yes
        return true;
    }

    private async Task Refresh()
    {
        var recordChunks = _storage.ReadRecords(LastRecordNumber);
        await foreach (var chunk in recordChunks)
        {
            var commands = chunk.Select(record => record.Command);
            Apply(commands);
        }
    }

    /// <summary>
    /// Execute a command  after writing it to storage
    /// </summary>
    internal async Task<object> ExecuteCommand(Command command)
    {
        var (predecessors, successors) = await Write(command);

        Apply(predecessors);
        var (result, events) = _kernel.Execute(command);
        NotifyCommandExecuted(command, isLocal: true, events);
        Apply(successors);
        return result;
    }

    private void Apply(IEnumerable<Command> commands)
    {
        foreach (var command in commands)
        {
           var (_, events) =  _kernel.Execute(command);
           NotifyCommandExecuted(command, isLocal: false, events);
        }
    }

    /// <summary>
    /// Write a command to storage and return any commands
    /// written by other engines that we need to apply before or after that we have n
    /// </summary>
    /// <param name="command"></param>
    /// <returns></returns>
    private async Task<(Command[] Predecessors,  Command[] Successors)> Write(Command command)
    {
        var expectedRecordNumber = _lastRecordNumber + 1;
        var record = await _storage.Append(command);
        if (record.RecordNumber != expectedRecordNumber)
        {
            var records = await ReadRecordsFrom(expectedRecordNumber);

            var predecessors = records
                .TakeWhile(r => r.RecordNumber < record.RecordNumber)
                .Select(r => r.Command)
                .ToArray();
            var successors = records
                .Skip(predecessors.Length + 1)
                .Select(r => r.Command)
                .ToArray();

            return (predecessors, successors);
        }

        return (Array.Empty<Command>(), Array.Empty<Command>());
    }

    private async Task<List<JournalRecord>> ReadRecordsFrom(long recordNumber)
    {
        //todo: push this down into IStorage
        var records = new List<JournalRecord>();
        var chunks = _storage.ReadRecords(from: recordNumber);
        await foreach (var chunk in chunks)
        {
            records.AddRange(chunk);
        }
        return records;
    }

    private void NotifyCommandExecuted(Command command, bool isLocal, IEnumerable<Event> events)
    {
        try
        {
            CommandExecuted.Invoke(command, isLocal, events);
        }
        catch (Exception exception)
        {
            _logger.Error(exception, "Exception thrown in CommandExecuted handler.");
        }
    }
}
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Memstate.Logging;

namespace Memstate
{
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

        public event CommandExecuted CommandExecuted = delegate { };

        public Engine(
            EngineSettings settings,
            TModel model,
            long recordNumber)
        {
            _lastRecordNumber = recordNumber;
            _logger = LogProvider.GetCurrentClassLogger();
            _kernel = new Kernel(settings, model);
            _settings = settings;
            _metrics = Metrics.Provider.GetEngineMetrics();
            ExecutionContext.Current = new ExecutionContext(recordNumber);
        }

        public long LastRecordNumber => Interlocked.Read(ref _lastRecordNumber);

        public async Task<TResult> Execute<TResult>(Command<TModel, TResult> command)
        {
            return (TResult) await ExecuteUntyped(command);
        }

        public Task Execute(Command<TModel> command)
        {
            return ExecuteUntyped(command);
        }

        public Task<TResult> Execute<TResult>(Query<TModel, TResult> query)
        {
            EnsureOperational();
            var result = (TResult)ExecuteUntyped(query);
            return Task.FromResult(result);
        }

        public Task DisposeAsync()
        {
            _logger.Debug("Begin Dispose");
            _logger.Debug("End Dispose");
            return Task.CompletedTask;
        }
        
        internal object ExecuteUntyped(Query query)
        {
            EnsureOperational();

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
        /// Execute non-generically typed command
        /// </summary>
        internal async Task<object> ExecuteUntyped(Command command)
        {
            return null;
        }
        
        private void EnsureOperational()
        {
            if (_stopped)
            {
                throw new Exception("Engine has stopped due to a failure condition");
            }
        }

        private void NotifyCommandExecuted(JournalRecord journalRecord, bool isLocal, IEnumerable<Event> events)
        {
            try
            {
                CommandExecuted.Invoke(journalRecord, isLocal, events);
            }
            catch (Exception exception)
            {
                _logger.Error(exception, "Exception thrown in CommandExecuted handler.");
            }
        }
    }
}
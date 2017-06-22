using System.Threading;

namespace Memstate.Core
{

    /// <summary>
    /// Encapsulates the in-memory object graph,
    /// executes commands and queries
    /// </summary>
    public class Kernel
    {
        private readonly object _model;
        private readonly ReaderWriterLockSlim _lock;
        private ulong _version;

        public Kernel(object model, ulong version)
        {
            _model = model;
            _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
            _version = version;
        }

        public ulong Version => _version;

        public object Execute(Command command)
        {
            try
            {
                _lock.EnterWriteLock();
                _version++;
                return command.ExecuteImpl(_model);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public object Execute(Query query)
        {
            try
            {
                _lock.EnterReadLock();
                return query.ExecuteImpl(_model);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
    }
}
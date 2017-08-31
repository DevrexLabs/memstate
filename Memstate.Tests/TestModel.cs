using System;
using System.Collections.Generic;

namespace Memstate.Tests
{
    public interface ITestModel
    {
        IEnumerable<Customer> Customers { get; }
        int CommandsExecuted { get; set; }
        bool OnLoadExecuted { get; }
        Customer this[int customerId] { get; set; }
        int ThrowCommandAborted();

        /// <summary>
        /// This will be a Command if called via Proxy
        /// </summary>
        void IncreaseNumber();

        /// <summary>
        /// This will be a CommandWithResult if called via Proxy
        /// </summary>
        /// <param name="livedb"></param>
        /// <returns></returns>
        string Uppercase(string livedb);

        Customer[] GetCustomers();
        Customer[] GetCustomersCloned();

        /// <summary>
        /// This is only for test and should return SerializationException since we can't use IEnumerable with yield.
        /// </summary>
        /// <returns></returns>
        IEnumerable<string> GetNames();

        /// <summary>
        /// This will be a Query if called via Proxy.
        /// </summary>
        /// <returns></returns>
        int GetCommandsExecuted();

        void AddCustomer(string name);
        void GenericCommand<T>(T item);
        R ComplexGeneric<T, R>(KeyValuePair<T,R> pair );
        T GenericQuery<T>(T item);
        T GenericQuery<T>(T item, int s);
        int DefaultArgs(int a, int b, int c = 42);
        T ExplicitGeneric<T>();
    }

    [Serializable]
    public class TestModel : ITestModel
    {
        private List<Customer> _customers = new List<Customer>();

        public IEnumerable<Customer> Customers
        {
            get
            {
                foreach (Customer customer in _customers)
                {
                    yield return customer;
                }
            }
        }

        public Customer this[int customerId]
        {
            get { return _customers[customerId]; }
            set
            {
                CommandsExecuted++;
                _customers[customerId] = value;
            }
        }

        public int ThrowCommandAborted()
        {
            throw new CommandAbortedException();
        }

        public int CommandsExecuted { get; set; }

        public bool OnLoadExecuted { get; private set; }


        /// <summary>
        /// This will be a Command if called via Proxy
        /// </summary>
        public void IncreaseNumber()
        {
            CommandsExecuted++;
        }

        /// <summary>
        /// This will be a CommandWithResult if called via Proxy
        /// </summary>
        /// <param name="livedb"></param>
        /// <returns></returns>
        [Command(IsolationLevel.InputOutput)]
        public string Uppercase(string livedb)
        {
            CommandsExecuted++;
            return livedb.ToUpper();
        }

        public Customer[] GetCustomers()
        {
            return _customers.ToArray();
        }

        [Query(Isolation = IsolationLevel.Output)]
        public Customer[] GetCustomersCloned()
        {
            return _customers.ToArray();
        }

        /// <summary>
        /// This is only for test and should return SerializationException since we can't use IEnumerable with yield.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<string> GetNames()
        {
            for (int i = 0; i < 10; i++)
            {
                yield return i.ToString();
            }
        }

        /// <summary>
        /// This will be a Query if called via Proxy.
        /// </summary>
        /// <returns></returns>
        public int GetCommandsExecuted()
        {
            return CommandsExecuted;
        }

        public void AddCustomer(string name)
        {
            CommandsExecuted++;
            _customers.Add(new Customer{Name = name});
        }

        public void GenericCommand<T>(T item)
        {
            CommandsExecuted++;
        }

        [Command]
        public R ComplexGeneric<T, R>(KeyValuePair<T,R> pair )
        {
            CommandsExecuted++;
            return pair.Value;
        }

        public T GenericQuery<T>(T item)
        {
            return item;
        }

        public T GenericQuery<T>(T item, int s)
        {
            return default(T);
        }

        public int DefaultArgs(int a, int b, int c = 42)
        {
            return a + b + c;
        }

        public T ExplicitGeneric<T>()
        {
            return default(T);
        }
    }

	[Serializable]
    public class GetNumberOfCommandsExecutedQuery : Query<TestModel, int>
    {
	    public GetNumberOfCommandsExecutedQuery()
	    {
	        ResultIsIsolated = true;
	    }

        public override int Execute(TestModel model)
        {
            return model.CommandsExecuted;
        }
    }

    [Serializable]
    public class TestCommandWithoutResult : Command<TestModel>
    {
        public  override void Execute(TestModel model)
        {
            model.CommandsExecuted++;
        }
    }

    [Serializable]
    public class TestCommandWithResult : Command<TestModel, int>
    {
        public byte[] Payload { get; set; }
        public bool ThrowInPrepare { get; set; }
        public bool ThrowExceptionWhenExecuting { get; set; }
        public bool ThrowCommandAbortedWhenExecuting { get; set; }

        public override int Execute(TestModel model)
        {
            if (ThrowCommandAbortedWhenExecuting)
            {
                throw new CommandAbortedException();
            }
            if (ThrowExceptionWhenExecuting)
            {
                throw new Exception();
            }
            return ++model.CommandsExecuted;
        }

    }

    public class CommandAbortedException : Exception
    {
    }
}

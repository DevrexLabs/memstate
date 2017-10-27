using System;
using Xunit;

namespace Memstate.Tests.DispatchProxy
{
    public class ProxyExceptionTests
    {
        readonly ProxyExceptionTestModel _proxy;
        readonly Client<ProxyExceptionTestModel> _client;

        int _callsToExecuting, _callsToExecuted;

        public ProxyExceptionTests()
        {
            _client = null;
            _proxy = _client.GetDispatchProxy();
            _callsToExecuting = 0;
            _callsToExecuted = 0;
            //_engine.CommandExecuting += (sender, args) => _callsToExecuting++;
            //_engine.CommandExecuted += (sender, args) => _callsToExecuted++;
        }

        [Fact]
        public void CommandAbortedException()
        {
            try
            {
                _proxy.ModifyAndThrow(new CommandAbortedException());
            }
            catch (Exception)
            {
                Assert.Equal(1, _callsToExecuting);
                Assert.Equal(0, _callsToExecuted);

                // verify that the model wasn't rolled back
                Assert.Equal(1, _proxy.GetState());
                return;
            }
            Assert.True(false, "Expected exception");
        }

        [Fact]
        public void UnexpectedException()
        {
            try
            {
                _proxy.ModifyAndThrow(new ArgumentException());
            }
            catch (Exception ex)
            {
                //Assert.IsType<CommandFailedException>(ex);
                Assert.NotNull(ex.InnerException);
                Assert.IsType<ArgumentException>(ex.InnerException);
                Assert.Equal(1, _callsToExecuting);
                Assert.Equal(0, _callsToExecuted); 
                Assert.Equal(0, _proxy.GetState());
                return;
            }
            Assert.True(false, "Expected exception");
        }
    }


    [Serializable]
    public class ProxyExceptionTestModel
    {
        private int _state;
        public void ModifyAndThrow(Exception ex)
        {
            _state++;
            throw ex;
        }

        public int GetState()
        {
            return _state;
        }
    }
}
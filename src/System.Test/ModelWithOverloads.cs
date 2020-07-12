using System;
using System.Linq;

namespace Memstate.Test.DispatchProxy
{

    public class ModelWithOverloads : IModelWithOverloads
    {
        private int _calls;

        public int GetCalls()
        {
            return _calls;
        }

        public void Meth()
        {
            _calls++;
        }

        public int Meth(int num)
        {
            _calls++;
            return num + 1;
        }

        public int Meth(params int[] stuff)
        {
            _calls++;
            return stuff.Sum(_ => _);
        }

        public int Inc(int number, int with = 1)
        {
            return number + with;
        }

    }
}
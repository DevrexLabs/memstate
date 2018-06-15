using System;

namespace Memstate
{
    public class DefaultModelCreator : IModelCreator
    {
        public T Create<T>()
        {
            var model = Activator.CreateInstance<T>();

            return model;
        }
    }
}
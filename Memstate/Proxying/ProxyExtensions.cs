using System;
using System.Reflection;

namespace Memstate
{
    public static class ProxyExtensions
    {
        public static T GetDispatchProxy<T>(this Client<T> client)
        {
            if (!typeof(T).GetTypeInfo().IsInterface) throw new InvalidCastException("The model type must be an interface");
            object proxy = DispatchProxy.Create<T, DispatchProxy<T>>();
            ((DispatchProxy<T>)proxy).SetClient(client);
            return (T) proxy;
        }
    }
}
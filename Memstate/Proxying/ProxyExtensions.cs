using System.Reflection;

namespace Memstate
{
    public static class ProxyExtensions
    {
        public static T GetProxy<T>(this Client<T> client)
        {
            object proxy = DispatchProxy.Create<T, ModelProxy<T>>();
            ((ModelProxy<T>)proxy).SetClient(client);
            return (T) proxy;
        }
    }
}
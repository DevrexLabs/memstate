namespace Memstate.Tests.DispatchProxy
{
    internal interface IModelWithRefArg
    {
        void Method(ref int a);
    }
}
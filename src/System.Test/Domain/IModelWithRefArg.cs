namespace Memstate.Test.DispatchProxy
{
    internal interface IModelWithRefArg
    {
        void Method(ref int a);
    }
}
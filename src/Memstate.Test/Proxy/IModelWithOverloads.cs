namespace Memstate.Tests.DispatchProxy
{
    public interface IModelWithOverloads
    {
        int GetCalls();
        int Inc(int number, int with = 1);
        void Meth();
        int Meth(int num);
        int Meth(params int[] stuff);
    }
}
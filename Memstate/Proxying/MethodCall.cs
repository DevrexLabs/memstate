using System.Reflection;

namespace Memstate
{
    public class MethodCall
    {
        public MethodCall(MethodInfo targetTargetMethod, object[] args)
        {
            TargetMethod = targetTargetMethod;
            Args = args;
        }

        public readonly MethodInfo TargetMethod;
        public readonly object[] Args;
    }
}
using System.Reflection;

namespace Memstate
{
    internal class NotAllowedAttribute : OperationAttribute
    {
        public static readonly OperationAttribute Default = new NotAllowedAttribute();

        internal override OperationInfo<T> ToOperationInfo<T>(MethodInfo methodInfo)
        {
            return new NotAllowedOperation<T>(methodInfo, this);
        }
    }
}
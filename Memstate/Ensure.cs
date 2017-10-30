namespace Memstate
{
    using System;

    public static class Ensure
    {
        public static void NotNull(object o, string name)
        {
            if (o == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        public static void NotNullOrEmpty(string @string, string name)
        {
            if (string.IsNullOrWhiteSpace(@string))
            {
                throw new ArgumentException(name);
            }
        }

        public static void ResolvableTypeName(string typeName, string name)
        {
            var type = Type.GetType(typeName, throwOnError: false, ignoreCase: true);
            if (type == null)
            {
                var message = string.Format("{0} is not a valid type", typeName);
                throw new ArgumentException(message, name);
            }
        }

        public static void That(Func<bool> predicate, string errorMessage)
        {
            if (!predicate.Invoke())
            {
                throw new ArgumentException(errorMessage);
            }
        }
    }
}
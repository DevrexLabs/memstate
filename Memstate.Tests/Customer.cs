using System;

namespace Memstate.Tests
{
    [Serializable]
    public class Customer
    {
        public string Name { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}

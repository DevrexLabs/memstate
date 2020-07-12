using System;

namespace Memstate.Test
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

using System;

namespace Memstate
{
    [Flags]
    public enum IsolationLevel
    {
        Unknown = 0,
        Input = 1,
        Output = 2,
        InputOutput = 3
    }
}
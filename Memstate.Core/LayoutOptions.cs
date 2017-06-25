using System;

namespace Memstate.Core
{
    [Flags]
    public enum LayoutOptions : byte
    {
        None = 0,
        Encryted = 1,
        Compressed = 2,
        Checksum = 4,
        All = Encryted | Compressed | Checksum
    }
}
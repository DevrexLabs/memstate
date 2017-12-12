using System;

namespace Memstate.Tcp
{
    [Flags]
    internal enum PacketInfo : short
    {
        IsPartial = 1,
    }
}
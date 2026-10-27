using System;
using System.Collections.Generic;

namespace Bw.Entities.Network
{
    public readonly struct RoomSettings
    {
        public readonly string Level;
        public readonly Guid HostKey;
        public readonly IReadOnlyList<RoomBot> Bots;

        public RoomSettings(string level, Guid hostKey, IReadOnlyList<RoomBot> bots)
        {
            Level = level;
            HostKey = hostKey;
            Bots = bots;
        }
    }
}

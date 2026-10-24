using System;
using UnityEngine;

namespace Bw.Entities.Network.Rollback
{
    [Serializable]
    public sealed class WorldRollbackConfig
    {
        [Min(1)] public int MaxReplayTicks = 30;
        [Min(1)] public int UnchangedStateTicks = 10;

        public int HistoryTicks => MaxReplayTicks + 1;
    }
}

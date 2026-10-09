using System.Collections.Generic;

namespace Bw.Entities.Network.Prediction.Events.Requests
{
    public readonly struct OutcomeReport<TEffect> where TEffect : struct
    {
        public readonly int Tick;
        public readonly IReadOnlyList<AffectedTarget<ulong, TEffect>> Affected;

        public OutcomeReport(int tick, IReadOnlyList<AffectedTarget<ulong, TEffect>> affected)
        {
            Tick = tick;
            Affected = affected;
        }
    }
}

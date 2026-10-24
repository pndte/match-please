using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Mortality;

namespace Bw.UseCases.Combat.Weapon.Network.Prediction
{
    public readonly struct Strike
    {
        public readonly int Tick;
        public readonly IReadonlyCharacter Actor;
        public readonly IReadOnlyList<AffectedTarget<IReadonlyMortal, Impacts>> Affected;

        public Strike(int tick, IReadonlyCharacter actor, IReadOnlyList<AffectedTarget<IReadonlyMortal, Impacts>> affected)
        {
            Tick = tick;
            Actor = actor;
            Affected = affected;
        }
    }
}

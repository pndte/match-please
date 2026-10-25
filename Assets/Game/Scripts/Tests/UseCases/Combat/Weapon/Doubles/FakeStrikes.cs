using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Weapon.Network.Prediction;
using Bw.UseCases.Mortality;
using JetBrains.Collections.Viewable;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Weapon.Doubles
{
    public sealed class FakeStrikes : IStrikes
    {
        public ISource<Strike> Struck => _struck;

        private readonly Signal<Strike> _struck = new();

        public void Strike(Strike strike) =>
            _struck.Fire(strike);

        public static Strike Of(int tick, IReadonlyCharacter actor, params IReadonlyMortal[] targets)
        {
            var affected = new List<AffectedTarget<IReadonlyMortal, Impacts>>();
            for (var index = 0; index < targets.Length; index++)
                affected.Add(new AffectedTarget<IReadonlyMortal, Impacts>(targets[index], ImpactsOf(index + 1f)));

            return new Strike(tick, actor, affected);
        }

        public static Impacts ImpactsOf(float damage) =>
            new(new Impact(new Hit(damage, Vector2.right * damage), new Contact(Vector2.up)));
    }
}

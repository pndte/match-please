using Bw.Entities.Extensions;
using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Combat.View.Impact;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Combat.View.Rig
{
    public sealed class StrikeHitStop
    {
        private readonly Animator _rig;
        private readonly StrikeFeelConfig _config;
        private readonly SequentialLifetimes _stops;

        public StrikeHitStop(
            Lifetime lifetime,
            Animator rig,
            IStrikeImpacts impacts,
            StrikeFeelConfig config)
        {
            _rig = rig;
            _config = config;
            _stops = new SequentialLifetimes(lifetime);

            impacts.Bodies.Advise(lifetime, _ => Stop());
        }

        private void Stop()
        {
            var stop = _stops.Next();
            _rig.speed = 0f;
            stop.OnTermination(() => _rig.speed = 1f);
            stop.WhenElapsed(_config.HitStop, _stops.TerminateCurrent);
        }
    }
}

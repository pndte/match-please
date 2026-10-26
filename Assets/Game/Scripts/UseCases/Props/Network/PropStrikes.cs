using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Weapon.Network.Prediction;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Movement;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Props.Network
{
    public sealed class PropStrikes : IStrikes
    {
        public ISource<Strike> Struck => _struck;

        private readonly Signal<Strike> _struck = new();
        private readonly List<AffectedTarget<IReadonlyMortal, Impacts>> _affected = new(4);
        private readonly List<IReadonlyMortal> _struckSinceRelease = new();
        private readonly PropReleaser _releaser;
        private readonly IReadonlySimulation<RigidState> _body;
        private readonly PhysicsBody _physics;
        private readonly IWorldStep _worldStep;
        private readonly PropThrowConfig _config;
        private readonly PhysicsMask _targets;

        public PropStrikes(
            Lifetime lifetime,
            PropReleaser releaser,
            IReadonlySimulation<RigidState> body,
            PhysicsBody physics,
            IWorldStep worldStep,
            PropThrowConfig config)
        {
            _releaser = releaser;
            _body = body;
            _physics = physics;
            _worldStep = worldStep;
            _config = config;
            _targets = config.Targets.AsPhysicsMask();

            releaser.Released.Advise(lifetime, _ => _struckSinceRelease.Clear());
            worldStep.Stepped(TickPhase.Late).Advise(lifetime, Detect);
        }

        private void Detect(int tick)
        {
            if (_worldStep.Replaying.Value || !_releaser.TryGetReleaser(out var releaser))
                return;

            FindStruck(releaser);
            if (_affected.Count > 0)
                _struck.Fire(new Strike(tick, releaser, _affected.ToArray()));
        }

        private void FindStruck(IReadonlyCharacter releaser)
        {
            _affected.Clear();
            var direction = _body.Previous.Velocity.normalized;
            var hits = _physics.world.contactHitEvents;
            for (var index = 0; index < hits.Length; index++)
            {
                var hit = hits[index];
                if (hit.approachSpeed < _config.MinHitSpeed || !TryGetStruck(hit, out var struck) || !struck.TryGetMortal(out var target))
                    continue;

                if (target == releaser.Mortal || target.State.Value == LifeState.Dead || _struckSinceRelease.Contains(target))
                    continue;

                _struckSinceRelease.Add(target);
                var blow = new Impact(new Hit(DamageAt(hit.approachSpeed), direction * _config.Knockback), struck.ContactAt(hit.point));
                _affected.Add(new AffectedTarget<IReadonlyMortal, Impacts>(target, new Impacts(blow)));
            }
        }

        private bool TryGetStruck(PhysicsEvents.ContactHitEvent hit, out PhysicsShape struck)
        {
            var own = hit.shapeA.body.Equals(_physics);
            struck = own ? hit.shapeB : hit.shapeA;
            return (own || hit.shapeB.body.Equals(_physics)) && (struck.contactFilter.categories.bitMask & _targets.bitMask) != 0;
        }

        private float DamageAt(float approachSpeed) =>
            Mathf.Clamp(_config.Damage * approachSpeed / _config.Speed, _config.MinDamage, _config.MaxDamage);
    }
}

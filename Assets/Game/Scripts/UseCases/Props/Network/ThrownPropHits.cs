using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Movement;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Props.Network
{
    public sealed class ThrownPropHits : IOutcomes<IReadonlyWeapon, IReadonlyMortal, Impacts>
    {
        private const float Upward = 0.7f;

        public ISource<ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Impacts>> Reported => _reported;

        private readonly Signal<ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Impacts>> _reported = new();
        private readonly List<AffectedTarget<IReadonlyMortal, Impacts>> _affected = new(4);
        private readonly IReadonlyWeapon _prop;
        private readonly IReadonlySimulation<RigidState> _body;
        private readonly PhysicsBody _physics;
        private readonly INetworkTicks _ticks;
        private readonly PropThrowConfig _config;
        private readonly PhysicsMask _targets;
        private readonly SequentialLifetimes _flights;

        public ThrownPropHits(
            Lifetime lifetime,
            IReadonlyWeapon prop,
            IReadonlySimulation<RigidState> body,
            PhysicsBody physics,
            INetworkTicks ticks,
            IWeaponHold hold,
            PropThrowConfig config)
        {
            _prop = prop;
            _body = body;
            _physics = physics;
            _ticks = ticks;
            _config = config;
            _targets = config.Targets.AsPhysicsMask();
            _flights = new SequentialLifetimes(lifetime);

            hold.HeldLifetime.WhenAlive(lifetime, _ => _flights.TerminateCurrent());
        }

        public void Track(IReadonlyMortal thrower)
        {
            var flight = _flights.Next();
            _physics.SetHitEvents(true);
            flight.OnTermination(() =>
            {
                if (_physics.isValid)
                    _physics.SetHitEvents(false);
            });
            _ticks.Ticked(TickPhase.Late).Advise(flight, tick => Check(tick, thrower));
        }

        private void Check(int tick, IReadonlyMortal thrower)
        {
            FindStruck(thrower);
            if (_affected.Count > 0)
            {
                var affected = _affected.ToArray();
                _flights.TerminateCurrent();
                _reported.Fire(new ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Impacts>(_prop, tick, affected));
                return;
            }

            if (Landed())
                _flights.TerminateCurrent();
        }

        private void FindStruck(IReadonlyMortal thrower)
        {
            _affected.Clear();
            var direction = _body.Previous.Velocity.normalized;
            var hits = _physics.world.contactHitEvents;
            for (var index = 0; index < hits.Length; index++)
            {
                var hit = hits[index];
                if (!TryGetStruck(hit, out var struck) || !struck.TryGetMortal(out var target))
                    continue;

                if (target == thrower || target.State.Value == LifeState.Dead || Contains(target))
                    continue;

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

        private bool Landed()
        {
            using var contacts = _physics.GetContacts(Allocator.Temp);
            for (var index = 0; index < contacts.Length; index++)
            {
                var contact = contacts[index];
                var upward = contact.shapeA.body.Equals(_physics) ? -contact.manifold.normal.y : contact.manifold.normal.y;
                if (contact.manifold.pointCount > 0 && upward >= Upward)
                    return true;
            }

            return false;
        }

        private bool Contains(IReadonlyMortal target)
        {
            for (var index = 0; index < _affected.Count; index++)
                if (_affected[index].Target == target)
                    return true;

            return false;
        }
    }
}

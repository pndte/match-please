using System.Collections.Generic;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Mortality;
using Bw.UseCases.Movement;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Props.Network
{
    public sealed class ThrownPropHits : IOutcomes<IReadonlyWeapon, IReadonlyMortal, Hit>
    {
        private const float Reach = 0.05f;
        private const float Upward = 0.7f;

        public ISource<ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Hit>> Reported => _reported;

        private readonly Signal<ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Hit>> _reported = new();
        private readonly List<Collider2D> _touched = new(8);
        private readonly List<AffectedTarget<IReadonlyMortal, Hit>> _affected = new(4);
        private readonly IReadonlyWeapon _prop;
        private readonly IReadonlySimulation<RigidState> _body;
        private readonly PhysicsBody _physics;
        private readonly INetworkTicks _ticks;
        private readonly PropThrowConfig _config;
        private readonly PhysicsMask _targetCategories;
        private readonly ContactFilter2D _targets;
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
            _targetCategories = config.Targets.AsPhysicsMask();
            _targets = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = config.Targets,
                useTriggers = false
            };
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
                _reported.Fire(new ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Hit>(_prop, tick, affected));
                return;
            }

            if (Landed())
                _flights.TerminateCurrent();
        }

        private void FindStruck(IReadonlyMortal thrower) //TODO: кого задело, ищется классическим коллайдером в точке удара — тело Box2D пока не знает, чьё оно; на этапе 4 цели опознаются по телам мира
        {
            _affected.Clear();
            var direction = _body.Previous.Velocity.normalized;
            var hits = _physics.world.contactHitEvents;
            for (var index = 0; index < hits.Length; index++)
            {
                var hit = hits[index];
                if (!StrikesTarget(hit))
                    continue;

                Physics2D.OverlapCircle(hit.point, Reach, _targets, _touched);
                for (var found = 0; found < _touched.Count; found++)
                {
                    var touched = _touched[found];
                    if (!touched.TryGetComponent<IHolder<IReadonlyMortal>>(out var holder))
                        continue;

                    var target = holder.Value;
                    if (target == thrower || target.State.Value == LifeState.Dead || Contains(target))
                        continue;

                    var struck = new Hit(DamageAt(hit.approachSpeed), direction * _config.Knockback, touched.transform.InverseTransformPoint(hit.point));
                    _affected.Add(new AffectedTarget<IReadonlyMortal, Hit>(target, struck));
                }
            }
        }

        private bool StrikesTarget(PhysicsEvents.ContactHitEvent hit)
        {
            var own = hit.shapeA.body.Equals(_physics);
            if (!own && !hit.shapeB.body.Equals(_physics))
                return false;

            var other = own ? hit.shapeB : hit.shapeA;
            return (other.contactFilter.categories.bitMask & _targetCategories.bitMask) != 0;
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

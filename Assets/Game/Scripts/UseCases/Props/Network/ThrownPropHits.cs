using System.Collections.Generic;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Mortality;
using Bw.UseCases.Movement;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Props.Network
{
    public sealed class ThrownPropHits : IOutcomes<IReadonlyWeapon, IReadonlyMortal, Hit>
    {
        private const float Reach = 0.05f;

        public ISource<ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Hit>> Reported => _reported;

        private readonly Signal<ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Hit>> _reported = new();
        private readonly List<Collider2D> _touched = new(8);
        private readonly List<AffectedTarget<IReadonlyMortal, Hit>> _affected = new(4);
        private readonly IReadonlyWeapon _prop;
        private readonly IReadonlySimulation<BodyState> _body;
        private readonly INetworkTicks _ticks;
        private readonly Transform _transform;
        private readonly BoxCollider2D _shape;
        private readonly PropThrowConfig _config;
        private readonly ContactFilter2D _targets;
        private readonly SequentialLifetimes _flights;

        public ThrownPropHits(
            Lifetime lifetime,
            IReadonlyWeapon prop,
            IReadonlySimulation<BodyState> body,
            INetworkTicks ticks,
            IWeaponHold hold,
            Transform transform,
            BoxCollider2D shape,
            PropThrowConfig config)
        {
            _prop = prop;
            _body = body;
            _ticks = ticks;
            _transform = transform;
            _shape = shape;
            _config = config;
            _targets = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = config.Targets,
                useTriggers = false
            };
            _flights = new SequentialLifetimes(lifetime);

            hold.HeldLifetime.WhenAlive(lifetime, _ => _flights.TerminateCurrent());
        }

        public void Track(IReadonlyMortal thrower) =>
            _ticks.Ticked(TickPhase.Late).Advise(_flights.Next(), tick => Check(tick, thrower));

        private void Check(int tick, IReadonlyMortal thrower)
        {
            var state = _body.State.Value;
            FindTouched(state.Position, thrower, Hit.Along(Heading(_body.Previous), _config)); //TODO: урон пока постоянный (PropThrowConfig.Damage) — должен зависеть от скорости сближения предмета и игрока
            if (_affected.Count > 0)
            {
                var affected = _affected.ToArray();
                _flights.TerminateCurrent();
                _reported.Fire(new ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Hit>(_prop, tick, affected));
                return;
            }

            if (state.Grounded)
                _flights.TerminateCurrent();
        }

        private void FindTouched(Vector2 position, IReadonlyMortal thrower, Hit hit)
        {
            _affected.Clear();
            var scale = (Vector2)_transform.lossyScale;
            var center = position + Vector2.Scale(_shape.offset, scale);
            var size = Vector2.Scale(_shape.size, scale) + new Vector2(Reach, Reach) * 2f;
            Physics2D.OverlapBox(center, size, 0f, _targets, _touched);

            for (var index = 0; index < _touched.Count; index++)
            {
                if (!_touched[index].TryGetComponent<IHolder<IReadonlyMortal>>(out var holder))
                    continue;

                var target = holder.Value;
                if (target == thrower || target.State.Value == LifeState.Dead || Contains(target))
                    continue;

                _affected.Add(new AffectedTarget<IReadonlyMortal, Hit>(target, hit));
            }
        }

        private bool Contains(IReadonlyMortal target)
        {
            for (var index = 0; index < _affected.Count; index++)
                if (_affected[index].Target == target)
                    return true;

            return false;
        }

        private static Vector2 Heading(BodyState body) =>
            new Vector2(body.HorizontalPush + body.Velocity.x, body.Velocity.y).normalized;
    }
}

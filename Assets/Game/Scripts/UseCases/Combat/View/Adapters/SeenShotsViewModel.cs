using Bw.Entities.Extensions;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Combat.View.Adapters
{
    public sealed class SeenShotsViewModel<TState> : IStateView<TState>, IShotsViewModel where TState : struct, IWeaponState
    {
        public ISource<SeenShot> Seen => _seen;

        private readonly Signal<SeenShot> _seen = new();
        private readonly IShotTracer _tracer;

        private bool _drawn;
        private bool _synchronized;
        private int _shots;

        public SeenShotsViewModel(
            Lifetime lifetime,
            IWeapon weapon,
            IWeaponsInHands inHands,
            IShotTracer tracer)
        {
            _tracer = tracer;

            inHands.ByWeapon.ViewForKey(lifetime, weapon, (drawnLifetime, _) =>
            {
                _drawn = true;
                drawnLifetime.OnTermination(() => _drawn = false);
            });
        }

        public void Show(TState from, TState to, float progress)
        {
            var fired = _synchronized && to.Shots > _shots; //TODO: несколько выстрелов между двумя кадрами (низкий FPS) видны как один, с последним стволом: отдача и трассер проиграются один раз
            _shots = to.Shots;
            _synchronized = true;

            if (fired && _drawn)
                See(to.Aim);
        }

        private void See(float aim)
        {
            var ray = _tracer.Aim(aim);
            var pellets = new PelletTrace[_tracer.Pellets];
            for (var pellet = 0; pellet < pellets.Length; pellet++)
                pellets[pellet] = TraceOf(_tracer.Pellet(ray, pellet));

            _seen.Fire(new SeenShot(ray.Direction, pellets));
        }

        private PelletTrace TraceOf(ShotRay ray)
        {
            var trace = _tracer.Cast(ray);
            return trace.IsStruck
                ? new PelletTrace(trace.From, trace.To, ray.Direction, true, trace.Normal, trace.Shape.Layer())
                : new PelletTrace(trace.From, trace.To, ray.Direction, false, Vector2.zero, 0);
        }
    }
}

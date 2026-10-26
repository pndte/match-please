using Bw.Entities.Extensions;
using Bw.Entities.Physics.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Hands.Extensions;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Combat.View.Adapters.Extensions;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Mortality.Extensions;
using Bw.UseCases.Mortality.Guarding;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Combat.View.Adapters
{
    public sealed class SeenShotsViewModel<TState> : IStateView<TState>, IShotsViewModel where TState : struct, IWeaponState
    {
        public ISource<SeenShot> Seen => _seen;

        private readonly Signal<SeenShot> _seen = new();
        private readonly IWeapon _weapon;
        private readonly IReadonlyWeaponsInHands _inHands;
        private readonly IShotTracer _tracer;
        private readonly ICharacterViews _views;
        private readonly IReadonlyGuardCovers _covers;

        private bool _drawn;
        private bool _synchronized;
        private int _shots;

        public SeenShotsViewModel(
            Lifetime lifetime,
            IWeapon weapon,
            IReadonlyWeaponsInHands inHands,
            IShotTracer tracer,
            ICharacterViews views,
            IReadonlyGuardCovers covers)
        {
            _weapon = weapon;
            _inHands = inHands;
            _tracer = tracer;
            _views = views;
            _covers = covers;

            inHands.ByWeapon.ViewForKey(lifetime, weapon, (drawnLifetime, _) =>
            {
                _drawn = true;
                drawnLifetime.OnTermination(() => _drawn = false);
            });
        }

        public void Show(TState from, TState to, float progress)
        {
            var fired = _synchronized && to.Shots > _shots;
            _shots = to.Shots;
            _synchronized = true;

            if (fired && _drawn)
                See(to.Aim);
        }

        private void See(float aim)
        {
            var ray = Shifted(_tracer.Aim(aim), _views.ShownOffset(_inHands.HolderOf(_weapon)));
            var pellets = new PelletTrace[_tracer.Pellets];
            for (var pellet = 0; pellet < pellets.Length; pellet++)
                pellets[pellet] = TraceOf(_tracer.Pellet(ray, pellet));

            _seen.Fire(new SeenShot(ray.Direction, pellets));
        }

        private static ShotRay Shifted(ShotRay ray, Vector2 offset) =>
            new(ray.Pivot + offset, ray.Muzzle + offset, ray.Direction);

        private PelletTrace TraceOf(ShotRay ray)
        {
            var trace = _tracer.Cast(ray);
            return trace.IsStruck
                ? new PelletTrace(trace.From, trace.To, ray.Direction, EndOf(trace), trace.Normal, trace.Shape.Layer())
                : new PelletTrace(trace.From, trace.To, ray.Direction, PelletEnd.Missed, Vector2.zero, 0);
        }

        private PelletEnd EndOf(ShotTrace trace) =>
            trace.Shape.TryGetMortal(out var mortal) && _covers.CoverOf(mortal).InCover(trace.Shape.ContactAt(trace.To)) ? PelletEnd.Blocked : PelletEnd.Struck;
    }
}

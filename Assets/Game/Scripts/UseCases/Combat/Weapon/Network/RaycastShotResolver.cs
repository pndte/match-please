using System;
using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Network.Extensions;
using Bw.UseCases.Mortality;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class RaycastShotResolver : IRaycastShots, IOutcomes<IReadonlyWeapon, IReadonlyMortal, Impacts>
    {
        public ISource<ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Impacts>> Reported => _reported;

        private readonly Signal<ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Impacts>> _reported = new();

        private readonly List<RaycastShot> _shots = new();
        private readonly List<ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Impacts>> _outcomes = new();
        private readonly List<AffectedTarget<IReadonlyMortal, Impact>> _struck = new();
        private readonly List<AffectedTarget<IReadonlyMortal, Impacts>> _affected = new();

        private readonly ILagCompensator _lagCompensator;
        private readonly ICharacterRegistry _registry;
        private readonly IReadonlyPlayerDrivers _drivers;

        public RaycastShotResolver(
            Lifetime lifetime,
            IWorldStep worldStep,
            ILagCompensator lagCompensator,
            ICharacterRegistry registry,
            IReadonlyPlayerDrivers drivers)
        {
            _lagCompensator = lagCompensator;
            _registry = registry;
            _drivers = drivers;

            worldStep.Stepped(TickPhase.Late).Advise(lifetime, Resolve);
        }

        public void Submit(RaycastShot shot) =>
            _shots.Add(shot);

        private void Resolve(int tick)
        {
            try
            {
                for (var index = 0; index < _shots.Count; index++)
                    _outcomes.Add(Trace(tick, _shots[index]));

                for (var index = 0; index < _outcomes.Count; index++)
                    _reported.Fire(_outcomes[index]);
            }
            finally
            {
                _shots.Clear();
                _outcomes.Clear();
            }
        }

        private ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Impacts> Trace(int tick, RaycastShot shot)
        {
            var ray = shot.Tracer.Aim(shot.WeaponShot.Aim);
            var view = new ShooterView(tick - shot.Timing.ViewDelay, tick - shot.Timing.PredictedDelay, _drivers.Of(_registry.PlayerByCharacter[shot.Shooter]).Value);
            _struck.Clear();
            _affected.Clear();
            Lifetime.Using(rewindLifetime =>
            {
                _lagCompensator.Rewind(rewindLifetime, view);
                shot.Tracer.CastPellets(ray, shot.Hit, _struck);
            });

            _struck.GroupByTarget(_affected);

            var affected = _affected.Count > 0
                ? _affected.ToArray() //TODO: выделение массива на выстрел - плохо, в идеале пуллировать массивы и переиспользовать их
                : Array.Empty<AffectedTarget<IReadonlyMortal, Impacts>>();
            return new ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Impacts>(shot.Weapon, _registry.PlayerByCharacter[shot.Shooter], tick - shot.Timing.LateTicks, affected);
        }
    }
}

using System;
using System.Collections.Generic;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Mortality;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Network.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network
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

        public RaycastShotResolver(Lifetime lifetime, INetworkTicks ticks, ILagCompensator lagCompensator)
        {
            _lagCompensator = lagCompensator;

            ticks.Ticked(TickPhase.Late).Advise(lifetime, Resolve);
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
            _struck.Clear();
            _affected.Clear();
            Lifetime.Using(rewindLifetime =>
            {
                _lagCompensator.Rewind(rewindLifetime, new SeenTicks(tick - shot.WeaponShot.ViewDelay, tick - shot.WeaponShot.PredictedDelay)); //TODO: откатываются только позиции персонажей, а жизнь и смерть (слой трупа), потом и целость ящиков, берутся из настоящего: цель, умершая на сервере уже после тика, который видел стрелок, пропускает пулю насквозь, хотя на его экране она ещё стояла
                shot.Tracer.CastPellets(ray, shot.Hit, _struck);
            });

            _struck.GroupByTarget(_affected);

            var affected = _affected.Count > 0
                ? _affected.ToArray() //TODO: выделение массива на выстрел - плохо, в идеале пуллировать массивы и переиспользовать их
                : Array.Empty<AffectedTarget<IReadonlyMortal, Impacts>>();
            return new ActionOutcome<IReadonlyWeapon, IReadonlyMortal, Impacts>(shot.Weapon, tick - shot.WeaponShot.LateTicks, affected);
        }
    }
}

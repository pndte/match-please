using System;
using System.Collections.Generic;
using Bw.Entities;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Character;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public sealed class RaycastShotResolver : IRaycastShots, IOutcomes<IReadonlyWeapon, IReadonlyCharacter, float>
    {
        public ISource<ActionOutcome<IReadonlyWeapon, IReadonlyCharacter, float>> Reported => _reported;

        private readonly Signal<ActionOutcome<IReadonlyWeapon, IReadonlyCharacter, float>> _reported = new();
        private readonly List<RaycastShot> _shots = new();
        private readonly List<ActionOutcome<IReadonlyWeapon, IReadonlyCharacter, float>> _outcomes = new();
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

        private ActionOutcome<IReadonlyWeapon, IReadonlyCharacter, float> Trace(int tick, RaycastShot shot)
        {
            var ray = shot.Tracer.Aim(shot.WeaponShot.Aim);
            var hit = Lifetime.Using(rewindLifetime =>
            {
                _lagCompensator.Rewind(rewindLifetime, tick - shot.WeaponShot.ViewDelay); //TODO: откатываются только позиции персонажей, а жизнь и смерть (слой трупа), потом и целость ящиков, берутся из настоящего: цель, умершая на сервере уже после тика, который видел стрелок, пропускает пулю насквозь, хотя на его экране она ещё стояла
                return shot.Tracer.Cast(ray).Hit;
            });

            var affected = hit && hit.collider.TryGetComponent<IHolder<IReadonlyCharacter>>(out var characterHolder)
                ? new[] { new AffectedTarget<IReadonlyCharacter, float>(characterHolder.Value, shot.Damage) } //TODO: выделение массива на выстрел - плохо, в идеале пуллировать массивы и переиспользовать их
                : Array.Empty<AffectedTarget<IReadonlyCharacter, float>>();
            return new ActionOutcome<IReadonlyWeapon, IReadonlyCharacter, float>(shot.Weapon, tick, affected);
        }
    }
}

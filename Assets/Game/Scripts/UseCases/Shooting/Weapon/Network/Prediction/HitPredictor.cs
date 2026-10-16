using System.Collections.Generic;
using Bw.Entities;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Mortality;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Network.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network.Prediction
{
    public sealed class HitPredictor<TState> where TState : struct
    {
        private readonly IReadonlySimulation<TState> _simulation;
        private readonly IShotTracer _tracer;
        private readonly INetworkTicks _ticks;
        private readonly IScheduledCollection _scheduled;
        private readonly IShotPower<TState> _power;
        private readonly IOutcomePrediction<IReadonlyMortal, Impacts> _outcomePrediction;
        private readonly List<AffectedTarget<IReadonlyMortal, Impact>> _struck = new();
        private readonly List<AffectedTarget<IReadonlyMortal, Impacts>> _affected = new();

        public HitPredictor( //TODO: чрезвычайное к-во параметров
            Lifetime lifetime,
            IControlledBy controlledBy,
            IReadonlyWeapon weapon,
            IReadonlySimulation<TState> simulation,
            IShotTracer tracer,
            INetworkTicks ticks,
            IScheduledCollection scheduled,
            IShotPower<TState> power,
            IOutcomePrediction<IReadonlyMortal, Impacts> outcomePrediction)
        {
            _simulation = simulation;
            _tracer = tracer;
            _ticks = ticks;
            _scheduled = scheduled;
            _power = power;
            _outcomePrediction = outcomePrediction;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                weapon.Fired.Advise(controlledLifetime, Predict));
        }

        private void Predict(WeaponShot shot)
        {
            var ray = _tracer.Aim(shot.Aim);
            _struck.Clear();
            _affected.Clear();
            _scheduled.PoseAt(_ticks.Current - (double)shot.PredictedDelay);
            try
            {
                _tracer.CastPellets(ray, _power.Along(ray.Direction, _simulation.State.Value), _struck);
            }
            finally
            {
                _scheduled.PoseAt(_ticks.Current);
            }

            _struck.GroupByTarget(_affected);

            for (var index = 0; index < _affected.Count; index++)
                _outcomePrediction.Predict(_ticks.Current, _affected[index].Target, _affected[index].Effect); //TODO: удар показывается в тик выстрела, а кровь — когда долетит трассер (ShotVfxConfig.BulletSpeed): издалека цель мигает раньше, чем в неё попадает пуля; отложить можно только показ, записать удар в предсказание итогов нужно сразу — итог может прийти раньше пули
        }
    }
}

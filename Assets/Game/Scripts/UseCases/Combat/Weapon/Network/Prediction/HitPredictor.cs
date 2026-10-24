using System.Collections.Generic;
using Bw.Entities;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Bw.UseCases.Combat.Weapon.Network.Extensions;
using Bw.UseCases.Mortality;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network.Prediction
{
    public sealed class HitPredictor<TState> : IStrikes where TState : struct
    {
        public ISource<Strike> Struck => _struck;

        private readonly Signal<Strike> _struck = new();
        private readonly List<TimedShot> _shots = new();
        private readonly List<AffectedTarget<IReadonlyMortal, Impact>> _hits = new();
        private readonly List<AffectedTarget<IReadonlyMortal, Impacts>> _affected = new();

        private readonly IReadonlySimulation<TState> _simulation;
        private readonly IShotTracer _tracer;
        private readonly IWorldStep _worldStep;
        private readonly IShotPower<TState> _power;

        public HitPredictor( //TODO: чрезвычайное к-во параметров
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            ShotTimer timer,
            IReadonlySimulation<TState> simulation,
            IShotTracer tracer,
            IWorldStep worldStep,
            IShotPower<TState> power)
        {
            _simulation = simulation;
            _tracer = tracer;
            _worldStep = worldStep;
            _power = power;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
            {
                timer.Timed.Advise(controlledLifetime, _shots.Add);
                worldStep.Stepped(TickPhase.Late).Advise(controlledLifetime, Resolve);
                controlledLifetime.OnTermination(_shots.Clear);
            });
        }

        private void Resolve(int tick)
        {
            try
            {
                if (!_worldStep.Replaying.Value)
                    Decide(tick);
            }
            finally
            {
                _shots.Clear();
            }
        }

        private void Decide(int tick)
        {
            for (var index = 0; index < _shots.Count; index++)
            {
                var shot = _shots[index];
                var ray = _tracer.Aim(shot.Shot.Aim);
                _hits.Clear();
                _affected.Clear();
                _tracer.CastPellets(ray, _power.Along(ray.Direction, _simulation.State.Value), _hits); //TODO: свой выстрел проверяется по миру клиента: предсказанные цели стоят в настоящем, остальные там, где их показывает интерполяция, а сервер судит интерполированных по откату к тику вида стрелка — между кадрами вида и тиком шага они расходятся на долю тика

                _hits.GroupByTarget(_affected);
                if (_affected.Count > 0)
                    _struck.Fire(new Strike(tick, shot.Shooter, _affected.ToArray())); //TODO: удар показывается в тик выстрела, а кровь — когда долетит трассер (ShotVfxConfig.BulletSpeed): издалека цель мигает раньше, чем в неё попадает пуля; отложить можно только показ, записать удар в предсказание итогов нужно сразу — итог может прийти раньше пули
            }
        }
    }
}

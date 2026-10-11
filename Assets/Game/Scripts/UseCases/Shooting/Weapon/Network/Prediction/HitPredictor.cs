using System.Collections.Generic;
using Bw.Entities;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Character;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Shooting.Weapon.Network.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network.Prediction
{
    public sealed class HitPredictor
    {
        private readonly IShotTracer _tracer;
        private readonly INetworkTicks _ticks;
        private readonly IHitConfig _config;
        private readonly IOutcomePrediction<IReadonlyCharacter, Hit> _outcomePrediction;
        private readonly List<AffectedTarget<IReadonlyCharacter, Hit>> _affected = new();

        public HitPredictor(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IReadonlyWeapon weapon,
            IShotTracer tracer,
            INetworkTicks ticks,
            IHitConfig config,
            IOutcomePrediction<IReadonlyCharacter, Hit> outcomePrediction)
        {
            _tracer = tracer;
            _ticks = ticks;
            _config = config;
            _outcomePrediction = outcomePrediction;

            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                weapon.Fired.Advise(controlledLifetime, Predict));
        }

        private void Predict(WeaponShot shot)
        {
            var ray = _tracer.Aim(shot.Aim);
            _affected.Clear();
            _tracer.CastPellets(ray, Hit.Along(ray.Direction, _config), _affected);

            for (var index = 0; index < _affected.Count; index++)
                _outcomePrediction.Predict(_ticks.Current, _affected[index].Target, _affected[index].Effect); //TODO: удар показывается в тик выстрела, а кровь — когда долетит трассер (ShotVfxConfig.BulletSpeed): издалека цель мигает раньше, чем в неё попадает пуля; отложить можно только показ, записать удар в предсказание итогов нужно сразу — итог может прийти раньше пули
        }
    }
}

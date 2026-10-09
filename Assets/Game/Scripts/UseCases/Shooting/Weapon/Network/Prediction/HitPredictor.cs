using Bw.Entities;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Character;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network.Prediction
{
    public sealed class HitPredictor
    {
        private readonly IShotTracer _tracer;
        private readonly INetworkTicks _ticks;
        private readonly ShootingWeaponConfig _config;
        private readonly IOutcomePrediction<IReadonlyCharacter, float> _outcomePrediction;

        public HitPredictor(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy,
            IReadonlyWeapon weapon,
            IShotTracer tracer,
            INetworkTicks ticks,
            ShootingWeaponConfig config,
            IOutcomePrediction<IReadonlyCharacter, float> outcomePrediction)
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
            var hit = _tracer.Cast(_tracer.Aim(shot.Aim)).Hit;
            if (hit && hit.collider.TryGetComponent<IHolder<IReadonlyCharacter>>(out var character))
                _outcomePrediction.Predict(_ticks.Current, character.Value, _config.Damage); //TODO: удар показывается в тик выстрела, а кровь — когда долетит трассер (ShotVfxConfig.BulletSpeed): издалека цель мигает раньше, чем в неё попадает пуля; отложить можно только показ, записать удар в предсказание итогов нужно сразу — итог может прийти раньше пули
        }
    }
}

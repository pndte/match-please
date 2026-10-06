using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Pool;
using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.Graphics;
using Bw.UseCases.Shooting.View.Audio.Abstractions;
using Bw.UseCases.Character;
using Bw.UseCases.Shooting.View.Impact.Abstractions;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Cysharp.Threading.Tasks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View
{
    public sealed class WeaponShotEffectsView : IStateView<WeaponState>
    {
        private readonly IShotTracer _tracer;
        private readonly IShotVfxPlayer _vfxPlayer;
        private readonly IShotSfxPlayer _sfxPlayer;
        private readonly LineRendererVfxConfig _vfxConfig;
        private readonly IPool<IImpactEffect> _impacts;
        private readonly Lifetime _lifetime;

        private bool _held;
        private bool _remote;
        private bool _synchronized;
        private int _shots;

        public WeaponShotEffectsView(
            Lifetime lifetime,
            IReadonlyControlledBy controlledBy, //TODO: чрезвычайное к-во аргументов в конструкторе
            IReadonlyWeapon weapon,
            IWeaponHold hold,
            IShotTracer tracer,
            IShotVfxPlayer vfxPlayer,
            IShotSfxPlayer sfxPlayer,
            LineRendererVfxConfig vfxConfig,
            IPool<IImpactEffect> impacts)
        {
            _tracer = tracer;
            _vfxPlayer = vfxPlayer;
            _sfxPlayer = sfxPlayer;
            _vfxConfig = vfxConfig;
            _impacts = impacts;
            _lifetime = lifetime;

            hold.HeldLifetime.WhenAlive(lifetime, heldLifetime =>
            {
                _held = true;
                heldLifetime.OnTermination(() => _held = false);
            });
            controlledBy.Me.WhenTrue(lifetime, controlledLifetime =>
                weapon.Fired.Advise(controlledLifetime, shot => Play(shot.Aim)));
            controlledBy.Me.WhenFalse(lifetime, remoteLifetime =>
            {
                _remote = true;
                _synchronized = false;
                remoteLifetime.OnTermination(() => _remote = false);
            });
        }

        public void Show(WeaponState from, WeaponState to, float progress)
        {
            if (!_remote)
                return;

            var fired = _synchronized && to.Shots > _shots;
            _shots = to.Shots;
            _synchronized = true;

            if (fired && _held)
                Play(to.Aim);
        }

        private void Play(float aim)
        {
            var ray = _tracer.Aim(aim);
            var trace = _tracer.Cast(ray);
            _sfxPlayer.Play(trace);
            if (trace.From != trace.To)
                _vfxPlayer.Play(trace.From, trace.To).Forget();
            if (trace.Hit && trace.Hit.collider.TryGetComponent<IHolder<IReadonlyCharacter>>(out _)) //TODO: кровь рисуется по локальному трассеру, а урон считает сервер — изредка будет кровь без урона или урон без крови
                _impacts.OneShot(_lifetime).Play(trace.To, ray.Direction, Vector2.Distance(trace.From, trace.To) / _vfxConfig.TrailSpeed);
        }
    }
}

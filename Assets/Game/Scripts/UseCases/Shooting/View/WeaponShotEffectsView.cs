using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Pool;
using Bw.Entities.Pool.GameObjects;
using Bw.UseCases.Shooting.View.Abstractions;
using Bw.UseCases.Shooting.View.Audio.Abstractions;
using Bw.UseCases.Character;
using Bw.UseCases.Shooting.View.Impact;
using Bw.UseCases.Shooting.View.Tracer;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using Bw.UseCases.Vfx.View.Effects.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View
{
    public sealed class WeaponShotEffectsView
    {
        private readonly IShotTracer _tracer;
        private readonly IShotSfxPlayer _sfxPlayer;
        private readonly IPrefabPools _pools;
        private readonly ShotVfxConfig _vfx;
        private readonly ShotImpactConfig _impact;
        private readonly Lifetime _lifetime;

        public WeaponShotEffectsView(
            Lifetime lifetime,
            ISeenShots shots, //TODO: чрезвычайное к-во аргументов в конструкторе
            IShotTracer tracer,
            IShotSfxPlayer sfxPlayer,
            IPrefabPools pools,
            ShotVfxConfig vfx,
            ShotImpactConfig impact)
        {
            _tracer = tracer;
            _sfxPlayer = sfxPlayer;
            _pools = pools;
            _vfx = vfx;
            _impact = impact;
            _lifetime = lifetime;

            shots.Seen.Advise(lifetime, Play);
        }

        private void Play(float aim)
        {
            var ray = _tracer.Aim(aim);
            var trace = _tracer.Cast(ray);
            var arrival = Vector2.Distance(trace.From, trace.To) / _vfx.BulletSpeed;

            if (arrival > 0f)
                _pools.For<LineTracer>(_vfx.Tracer).Resource(_lifetime.CreateNestedFor(arrival)).Play(trace.From, trace.To, arrival);

            _sfxPlayer.Play(trace, arrival);

            if (trace.Hit && trace.Hit.collider.TryGetComponent<IHolder<IReadonlyCharacter>>(out _)) //TODO: кровь рисуется по локальному трассеру, а урон решает сервер (свой выстрел предсказан той же трассировкой, но сервер может его отменить) — изредка будет кровь без урона или урон без крови
                _lifetime.WhenElapsed(arrival, () => _pools.For<IEffect>(_impact.Prefab).OneShot(_lifetime).Play(trace.To, ray.Direction));
        }
    }
}

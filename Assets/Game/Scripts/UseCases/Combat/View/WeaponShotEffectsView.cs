using Bw.Entities.Extensions;
using Bw.Entities.Pool;
using Bw.Entities.Pool.GameObjects;
using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Combat.View.Audio.Abstractions;
using Bw.UseCases.Combat.View.Impact;
using Bw.UseCases.Vfx.View.Effects.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Combat.View
{
    public sealed class WeaponShotEffectsView
    {
        private readonly IShotSfxPlayer _sfxPlayer;
        private readonly IPrefabPools _pools;
        private readonly ShotVfxConfig _vfx;
        private readonly ShotImpactConfig _impact;
        private readonly Lifetime _lifetime;

        public WeaponShotEffectsView(
            Lifetime lifetime,
            IShotsViewModel shots,
            IShotSfxPlayer sfxPlayer,
            IPrefabPools pools,
            ShotVfxConfig vfx,
            ShotImpactConfig impact)
        {
            _sfxPlayer = sfxPlayer;
            _pools = pools;
            _vfx = vfx;
            _impact = impact;
            _lifetime = lifetime;

            shots.Seen.Advise(lifetime, Play);
        }

        private void Play(SeenShot shot)
        {
            _sfxPlayer.Play();

            for (var pellet = 0; pellet < shot.Pellets.Count; pellet++)
                PlayPellet(shot.Pellets[pellet]);
        }

        private void PlayPellet(PelletTrace trace)
        {
            var arrival = Vector2.Distance(trace.From, trace.To) / _vfx.BulletSpeed;

            if (arrival > 0f)
                _pools.For<IBulletTracer>(_vfx.Tracer).OneShot(_lifetime).Fly(trace.From, trace.To, _vfx.BulletSpeed);

            _sfxPlayer.PlayImpact(trace, arrival);

            if (!trace.Struck)
                return;

            if (IsBody(trace.Layer)) //TODO: кровь рисуется по локальному трассеру, а урон решает сервер (свой выстрел предсказан той же трассировкой, но сервер может его отменить) — изредка будет кровь без урона или урон без крови
                _lifetime.WhenElapsed(arrival, () => _pools.For<IEffect>(_impact.Prefab).OneShot(_lifetime).Play(trace.To, trace.Direction));
            else //TODO: любая поверхность бьётся одинаково (искры, серая крошка, пыль, дырка) — дереву, металлу и земле нужны свои частицы, как и звуки попадания; дырка остаётся там, куда попала пуля, и на едущем лифте, на отброшенном выстрелом или разбитом ящике повиснет в воздухе
                _lifetime.WhenElapsed(arrival, () => _pools.For<IEffect>(_vfx.Impact).OneShot(_lifetime).Play(trace.To, trace.Normal));
        }

        private bool IsBody(int layer) =>
            (_impact.BodyLayers.value & (1 << layer)) != 0;
    }
}

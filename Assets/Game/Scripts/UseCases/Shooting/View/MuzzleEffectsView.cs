using Bw.Entities.Pool;
using Bw.Entities.Pool.GameObjects;
using Bw.UseCases.Shooting.View.Abstractions;
using Bw.UseCases.Vfx.View.Effects.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View
{
    public sealed class MuzzleEffectsView
    {
        private const float EjectBack = 0.7f;

        private readonly Lifetime _lifetime;
        private readonly IMuzzleFlash _flash;
        private readonly IPrefabPools _pools;
        private readonly ShotVfxConfig _vfx;
        private readonly Transform _ejector;

        public MuzzleEffectsView(
            Lifetime lifetime,
            IShotsViewModel shots,
            IMuzzleFlash flash,
            IPrefabPools pools,
            ShotVfxConfig vfx,
            Transform ejector)
        {
            _lifetime = lifetime;
            _flash = flash;
            _pools = pools;
            _vfx = vfx;
            _ejector = ejector;

            shots.Seen.Advise(lifetime, _ => Play());
        }

        private void Play()
        {
            _flash.Play();
            _pools.For<IEffect>(_vfx.Casing).OneShot(_lifetime).Play(_ejector.position, (Vector2.up - (Vector2)_ejector.right * EjectBack).normalized); //TODO: гильза вылетает в момент выстрела у всех пушек — у дробовика и снайперки она должна вылетать при передёргивании помпы и затвора, когда у рига появится такая анимация
        }
    }
}

using Bw.Entities.Pool;
using Bw.Entities.Pool.GameObjects;
using Bw.UseCases.Vfx.View.Effects.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Vfx.View.Effects
{
    public sealed class EffectPlayer : IEffectPlayer
    {
        private readonly Lifetime _lifetime;
        private readonly IPrefabPools _pools;

        public EffectPlayer(Lifetime lifetime, IPrefabPools pools)
        {
            _lifetime = lifetime;
            _pools = pools;
        }

        public void Play(GameObject prefab, Vector2 point, Vector2 direction) =>
            _pools.For<IEffect>(prefab).OneShot(_lifetime).Play(point, direction);
    }
}

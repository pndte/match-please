using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Shooting.View
{
    public sealed class WeaponSpriteFlip
    {
        private readonly Transform _weapon;
        private readonly SpriteRenderer _sprite;

        public WeaponSpriteFlip(Lifetime lifetime, Transform weapon, SpriteRenderer sprite)
        {
            _weapon = weapon;
            _sprite = sprite;
            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate, lifetime).Subscribe(Flip);
        }

        private void Flip(Unit _) =>
            _sprite.flipY = _weapon.right.x < 0f;
    }
}

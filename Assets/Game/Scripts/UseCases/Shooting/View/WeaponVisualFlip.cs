using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Shooting.View
{
    public sealed class WeaponVisualFlip
    {
        private static readonly Vector3 Upright = Vector3.one;
        private static readonly Vector3 Mirrored = new(1f, -1f, 1f);

        private readonly Transform _weapon;
        private readonly Transform _visual;

        public WeaponVisualFlip(Lifetime lifetime, Transform weapon, Transform visual)
        {
            _weapon = weapon;
            _visual = visual;
            Observable.EveryUpdate(UnityFrameProvider.PostLateUpdate, lifetime).Subscribe(Flip);
        }

        private void Flip(Unit _) =>
            _visual.localScale = _weapon.right.x < 0f ? Mirrored : Upright;
    }
}

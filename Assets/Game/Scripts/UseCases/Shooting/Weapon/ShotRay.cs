using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon
{
    public readonly struct ShotRay
    {
        public readonly Vector2 Pivot;
        public readonly Vector2 Muzzle;
        public readonly Vector2 Direction;

        public ShotRay(Vector2 pivot, Vector2 muzzle, Vector2 direction)
        {
            Pivot = pivot;
            Muzzle = muzzle;
            Direction = direction;
        }
    }
}

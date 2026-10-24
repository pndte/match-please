using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Weapon;
using UnityEngine;

namespace Bw.UseCases.Combat.Shooting
{
    public sealed class WeaponMuzzle
    {
        private readonly Transform _muzzle;
        private readonly WeaponOrbit _orbit;

        public WeaponMuzzle(Transform muzzle, WeaponOrbit orbit)
        {
            _muzzle = muzzle;
            _orbit = orbit;
        }

        public ShotRay RayFrom(HoldPose hands, float aim)
        {
            var rotation = Quaternion.Euler(0f, 0f, aim);
            var offset = _muzzle.localPosition;
            var radius = _orbit.RadiusAt(hands.Position, aim);
            return new ShotRay(
                hands.Position,
                hands.Position + (Vector2)(rotation * new Vector3(radius + offset.x, offset.y)),
                rotation * Vector3.right);
        }
    }
}

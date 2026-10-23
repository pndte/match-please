using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Movement;
using UnityEngine;

namespace Bw.UseCases.Combat.Weapon
{
    public sealed class HeldWeaponPose
    {
        private readonly IReadonlySimulation<GunState> _gun;
        private readonly WeaponOrbit _orbit;

        public HeldWeaponPose(IReadonlySimulation<GunState> gun, WeaponOrbit orbit)
        {
            _gun = gun;
            _orbit = orbit;
        }

        public RigidState PoseIn(HoldPose hands)
        {
            var aim = _gun.State.Value.Aim;
            var direction = (Vector2)(Quaternion.Euler(0f, 0f, aim) * Vector3.right);
            return new RigidState(hands.Position + direction * _orbit.RadiusAt(hands.Position, aim), aim, Vector2.zero, 0f);
        }
    }
}

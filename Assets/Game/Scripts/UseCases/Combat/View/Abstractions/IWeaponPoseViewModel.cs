using UnityEngine;

namespace Bw.UseCases.Combat.View.Abstractions
{
    public interface IWeaponPoseViewModel
    {
        public Vector2 Pivot { get; }
        public float Aim { get; }
        public float Radius { get; }
        public float RadiusSpeed { get; }
    }
}

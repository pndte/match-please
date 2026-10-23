using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Combat.Weapon.Pickup.Abstractions
{
    public interface IFloorWeapon
    {
        public Vector2 Position { get; }
        public float DistanceTo(Vector2 point);
        public void Highlight(Lifetime highlightLifetime);
    }
}

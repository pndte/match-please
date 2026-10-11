using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Pickup.Abstractions
{
    public interface IFloorWeapon
    {
        public Vector2 Position { get; }
        public float DistanceTo(Vector2 point);
        public void Highlight(Lifetime highlightLifetime);
    }
}

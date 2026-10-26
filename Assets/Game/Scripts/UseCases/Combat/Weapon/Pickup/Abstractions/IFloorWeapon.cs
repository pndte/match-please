using Bw.UseCases.Combat.Abstractions.Weapons;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Combat.Weapon.Pickup.Abstractions
{
    public interface IFloorWeapon
    {
        public IWeapon Weapon { get; }
        public Vector2 Position { get; }
        public float DistanceTo(Vector2 point);
        public void Highlight(Lifetime highlightLifetime);
    }
}

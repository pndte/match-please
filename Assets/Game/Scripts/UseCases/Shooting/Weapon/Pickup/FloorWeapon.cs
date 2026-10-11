using Bw.UseCases.Shooting.Weapon.Pickup.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Pickup
{
    public sealed class FloorWeapon : IFloorWeapon
    {
        public Vector2 Position => _shape.bounds.center;

        private readonly Collider2D _shape;
        private readonly IWeaponHighlight _highlight;

        public FloorWeapon(Collider2D shape, IWeaponHighlight highlight)
        {
            _shape = shape;
            _highlight = highlight;
        }

        public float DistanceTo(Vector2 point) =>
            Vector2.Distance(point, _shape.ClosestPoint(point));

        public void Highlight(Lifetime highlightLifetime) =>
            _highlight.Show(highlightLifetime);
    }
}

using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Shooting.Weapon.Pickup.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Pickup
{
    public sealed class FloorWeapon : IFloorWeapon
    {
        public Vector2 Position => _box.WorldGeometry().centroid;

        private readonly BoxCollider2D _box;
        private readonly IWeaponHighlight _highlight;

        public FloorWeapon(BoxCollider2D box, IWeaponHighlight highlight)
        {
            _box = box;
            _highlight = highlight;
        }

        public float DistanceTo(Vector2 point) =>
            _box.WorldGeometry().DistanceTo(point);

        public void Highlight(Lifetime highlightLifetime) =>
            _highlight.Show(highlightLifetime);
    }
}

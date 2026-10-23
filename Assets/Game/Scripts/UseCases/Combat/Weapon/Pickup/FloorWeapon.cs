using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Combat.Weapon.Pickup.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Combat.Weapon.Pickup
{
    public sealed class FloorWeapon : IFloorWeapon
    {
        public Vector2 Position => _box.WorldGeometry().centroid;

        private readonly BodyBox _box;
        private readonly IWeaponHighlight _highlight;

        public FloorWeapon(BodyBox box, IWeaponHighlight highlight)
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

using System.Collections.Generic;
using Bw.UseCases.Combat.Weapon.Pickup.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.Tests.UseCases.Combat.Weapon.Doubles
{
    public sealed class FakeFloorWeapon : IFloorWeapon
    {
        public Vector2 Position { get; }
        public IReadOnlyList<Lifetime> Highlights => _highlights;

        private readonly List<Lifetime> _highlights = new();

        public FakeFloorWeapon(Vector2 position)
        {
            Position = position;
        }

        public float DistanceTo(Vector2 point) =>
            Vector2.Distance(Position, point);

        public void Highlight(Lifetime highlightLifetime) =>
            _highlights.Add(highlightLifetime);
    }
}

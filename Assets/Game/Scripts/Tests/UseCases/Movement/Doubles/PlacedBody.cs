using System.Collections.Generic;
using Bw.UseCases.Movement.Abstractions;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement.Doubles
{
    public sealed class PlacedBody : IBodyPicture
    {
        public Vector2 Position { get; private set; }
        public IReadOnlyList<Vector2> Placed => _placed;

        private readonly List<Vector2> _placed = new();

        public PlacedBody(Vector2 position = default)
        {
            Position = position;
        }

        public void Place(Vector2 position)
        {
            _placed.Add(position);
            Position = position;
        }
    }
}

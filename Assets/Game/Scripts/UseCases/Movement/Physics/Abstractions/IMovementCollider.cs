using System.Collections.Generic;
using UnityEngine;

namespace Bw.UseCases.Movement.Physics.Abstractions
{
    public interface IMovementCollider
    {
        public float Cast(Vector2 position, Vector2 direction, float distance);
        public float WallDistance(
            Vector2 position,
            Vector2 direction,
            float distance,
            IMovementCollider shover);
        public IReadOnlyList<ShovableContact> ShovablesAhead(
            Vector2 position,
            Vector2 direction,
            float distance,
            IMovementCollider shover);
        public float Fall(Vector2 position, float distance);
        public Vector2 Carry(Vector2 position, bool grounded);
        public bool TryGetLaunch(Vector2 position, out float height);
        public Vector2 RideShift(Vector2 position, double fromSeconds, double toSeconds);
        public bool Is(Collider2D collider);
    }
}

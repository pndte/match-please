using System.Collections.Generic;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Physics.Abstractions
{
    public interface IMovementCollider
    {
        public float Cast(Vector2 position, Vector2 direction, float distance);
        public IReadOnlyList<ShovableContact> ShovablesAhead(Vector2 position, Vector2 direction, float distance);
        public float Fall(Vector2 position, float distance);
        public Vector2 Carry(Vector2 position, bool grounded);
        public bool TryGetLaunch(Vector2 position, out float height);
        public bool Is(PhysicsBody body);
    }
}

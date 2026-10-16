using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Movement;
using Bw.UseCases.Props.Extensions;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Props.Network
{
    public sealed class ReleaseSpots
    {
        private const float AngleStep = 5f;
        private const float Tolerance = 0.005f;

        private readonly PhysicsWorld _world;
        private readonly PhysicsBody _body;
        private readonly Vector2 _size;
        private readonly Vector2 _offset;
        private readonly PhysicsQuery.QueryFilter _filter;
        private readonly HeldPropConfig _config;

        public ReleaseSpots(
            IPhysicsSpace space,
            PhysicsBody body,
            BoxCollider2D shape,
            DynamicBodyConfig bodyConfig,
            HeldPropConfig config)
        {
            _world = space.World;
            _body = body;
            _size = shape.WorldSize();
            _offset = shape.WorldOffset();
            _filter = new PhysicsQuery.QueryFilter(PhysicsMask.All, bodyConfig.Contacts.AsPhysicsMask());
            _config = config;
        }

        public Vector2 Nearest(Rect holder, Vector2 held) //TODO: вокруг держателя нет свободного места — предмет отпускается в руках, и физика выталкивает его как получится
        {
            if (IsFree(held + _offset))
                return held;

            var aim = Vector2.SignedAngle(Vector2.right, held + _offset - holder.center);
            for (var turn = AngleStep; turn <= 180f; turn += AngleStep)
                if (TryGetFree(holder, aim + turn, out var spot) || TryGetFree(holder, aim - turn, out spot))
                    return spot;

            return held;
        }

        private bool TryGetFree(Rect holder, float aim, out Vector2 spot)
        {
            var center = holder.center + _config.HeldOffset(aim, holder.size / 2f, _size / 2f);
            spot = center - _offset;
            return IsFree(center);
        }

        private bool IsFree(Vector2 center) =>
            _world.IsFree(PolygonGeometry.CreateBox(_size - Vector2.one * (Tolerance * 2f), 0f, new PhysicsTransform(center), false), _filter, _body);
    }
}

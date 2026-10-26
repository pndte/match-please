using System.Collections.Generic;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Spawning.Network
{
    public sealed class CrateSpots
    {
        public IReadOnlyList<Transform> All => _spots;

        private readonly CrateSpawnConfig _config;
        private readonly Transform[] _spots;
        private readonly PhysicsWorld _world;

        public CrateSpots(CrateSpawnConfig config, Transform[] spots, IPhysicsSpace space)
        {
            _config = config;
            _spots = spots;
            _world = space.World;
        }

        public bool IsFree(Transform spot)
        {
            var box = _config.Prefab.GetComponent<BodyBox>();
            var rotation = spot.rotation;
            var center = (Vector2)spot.position + (Vector2)(rotation * box.WorldOffset());
            var pose = new PhysicsTransform(center, new PhysicsRotate(rotation.eulerAngles.z * Mathf.Deg2Rad));
            var shape = PolygonGeometry.CreateBox(box.WorldSize() - Vector2.one * (PhysicsSlop.Linear * 2f), 0f, pose, false);
            return _world.IsFree(shape, new PhysicsQuery.QueryFilter(PhysicsMask.All, _config.Blockers.AsPhysicsMask()));
        }
    }
}

using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Combat.Weapon
{
    public sealed class WeaponOrbit
    {
        private readonly WeaponRotationConfig _config;
        private readonly PhysicsWorld _world;
        private readonly PhysicsQuery.QueryFilter _obstacles;

        public WeaponOrbit(WeaponRotationConfig config, IPhysicsSpace space)
        {
            _config = config;
            _world = space.World;
            _obstacles = new PhysicsQuery.QueryFilter(PhysicsMask.All, config.ObstacleLayerMask.AsPhysicsMask());
        }

        public float RadiusAt(Vector2 pivot, float aim)
        {
            var reach = (Vector2)(Quaternion.Euler(0f, 0f, aim) * Vector3.right) * _config.ObstacleDetectionDistance;
            using var obstacles = _world.CastRay(new PhysicsQuery.CastRayInput(pivot, reach), _obstacles, PhysicsQuery.WorldCastMode.AllSorted, Allocator.Temp);
            foreach (var obstacle in obstacles)
                if (!obstacle.shape.IsOneWay())
                    return _config.MinOrbitRadius;

            return _config.MaxOrbitRadius;
        }
    }
}

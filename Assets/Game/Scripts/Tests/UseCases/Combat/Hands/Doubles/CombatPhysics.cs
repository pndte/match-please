using System;
using Bw.Entities.Physics;
using Bw.Tests.Doubles;
using JetBrains.Lifetimes;
using Unity.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Combat.Hands.Doubles
{
    public sealed class CombatPhysics : IDisposable
    {
        public const int Character = 15;
        public const int HeldProp = 17;

        public Lifetime Lifetime => _lifetime.Lifetime;
        public HiddenObjects Objects { get; } = new();
        public PhysicsSpace Space { get; }

        private readonly LifetimeDefinition _lifetime = new();

        public CombatPhysics()
        {
            Space = PhysicsSpace.Create(_lifetime.Lifetime, AssetDatabase.LoadAssetAtPath<PhysicsConfig>("Assets/Resources/Physics/PhysicsConfig.asset"), new ManualClock());
        }

        public BodyBox Box(
            string name,
            Vector2 position,
            Vector2 size,
            int layer) =>
            Objects.Create(name, position, layer).AddComponent<BodyBox>().With("_size", size);

        public PhysicsShape[] ShapesAt(Vector2 point, int layer)
        {
            var mask = PhysicsMask.None;
            mask.SetBit(layer);
            using var found = Space.World.OverlapGeometry(new CircleGeometry { center = point, radius = 0.01f }, new PhysicsQuery.QueryFilter(PhysicsMask.All, mask), Allocator.Temp);
            var shapes = new PhysicsShape[found.Length];
            for (var index = 0; index < found.Length; index++)
                shapes[index] = found[index].shape;

            return shapes;
        }

        public void Dispose()
        {
            _lifetime.Terminate();
            Objects.Dispose();
        }
    }
}

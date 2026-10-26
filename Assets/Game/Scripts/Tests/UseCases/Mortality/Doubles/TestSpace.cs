using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Tests.Doubles;
using JetBrains.Lifetimes;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Mortality.Doubles
{
    public sealed class TestSpace
    {
        public PhysicsSpace Space { get; }

        private readonly Lifetime _lifetime;
        private readonly HiddenObjects _objects;

        public TestSpace(Lifetime lifetime, HiddenObjects objects)
        {
            _lifetime = lifetime;
            _objects = objects;
            var config = AssetDatabase.LoadAssetAtPath<PhysicsConfig>("Assets/Game/Content/Physics/PhysicsConfig.asset");
            Space = PhysicsSpace.Create(lifetime, config, new ManualClock());
        }

        public BodyBox Box(
            string name,
            Vector2 position,
            Vector2 size,
            Vector2 offset,
            int layer = 0)
        {
            var box = _objects.Create(name, position, layer).AddComponent<BodyBox>();
            return box.With("_size", size).With("_offset", offset);
        }

        public PhysicsBody Body(BodyBox box, PhysicsBody.BodyType type = PhysicsBody.BodyType.Dynamic) =>
            Space.CreateBody(_lifetime, box, type);
    }
}

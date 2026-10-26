using System;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.Tests.Doubles;
using JetBrains.Lifetimes;
using UnityEditor;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.Entities.Physics.Doubles
{
    public sealed class PhysicsTestWorld : IDisposable
    {
        public const int Ground = 14;
        public const int Character = 15;
        public const string ConfigPath = "Assets/Game/Content/Physics/PhysicsConfig.asset";

        public Lifetime Lifetime => _lifetime.Lifetime;
        public ManualClock Clock { get; } = new();
        public HiddenObjects Objects { get; } = new();
        public PhysicsSpace Space { get; }
        public PhysicsWorld World => Space.World;

        private readonly LifetimeDefinition _lifetime = new();

        public PhysicsTestWorld()
        {
            Space = PhysicsSpace.Create(_lifetime.Lifetime, AssetDatabase.LoadAssetAtPath<PhysicsConfig>(ConfigPath), Clock);
        }

        public BodyBox Box(
            string name,
            Vector2 position,
            Vector2 size,
            int layer = Ground) =>
            Box(name, position, size, Vector2.zero, layer);

        public BodyBox Box(
            string name,
            Vector2 position,
            Vector2 size,
            Vector2 offset,
            int layer)
        {
            var box = Objects.Create(name, position, layer).AddComponent<BodyBox>();
            return box.With("_size", size).With("_offset", offset);
        }

        public BodyBox Plank(string name, Vector2 position, Vector2 size) =>
            Box(name, position, size).With("_oneWay", true);

        public PhysicsBody Static(string name, Vector2 position, Vector2 size) =>
            Space.CreateBody(Lifetime, Box(name, position, size), PhysicsBody.BodyType.Static);

        public PhysicsBody StaticPlank(string name, Vector2 position, Vector2 size) =>
            Space.CreateBody(Lifetime, Plank(name, position, size), PhysicsBody.BodyType.Static);

        public PhysicsBody Dynamic(string name, Vector2 position, Vector2 size)
        {
            var body = PhysicsBodyDefinition.defaultDefinition;
            body.type = PhysicsBody.BodyType.Dynamic;
            var shape = PhysicsShapeDefinition.defaultDefinition;
            shape.preSolveCallbacks = true;
            return Space.CreateBody(Lifetime, Box(name, position, size), body, shape);
        }

        public void Step(int times = 1)
        {
            for (var index = 0; index < times; index++)
                Space.Step();
        }

        public void Dispose()
        {
            _lifetime.Terminate();
            Objects.Dispose();
        }
    }
}

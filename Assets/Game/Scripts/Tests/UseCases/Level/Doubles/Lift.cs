using Bw.Entities.Physics.Extensions;
using Bw.Tests.Entities.Physics.Doubles;
using Bw.UseCases.Level;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Level.Doubles
{
    public sealed class Lift
    {
        public static readonly Vector2 Start = new(2f, 3f);
        public static readonly Vector2 Size = new(3f, 0.5f);

        public PhysicsBody Body { get; }
        public PlatformRail Rail { get; }
        public MovingPlatform Platform { get; }

        public Lift(PhysicsTestWorld world, Vector2 travel)
        {
            var box = world.Box("Lift", Start, Size);
            Body = world.Space.CreateBody(world.Lifetime, box, PhysicsBody.BodyType.Kinematic);
            Rail = new PlatformRail(Start, travel);
            Platform = new MovingPlatform(world.Lifetime, Body, new[] { box }, Rail, world.Clock, world.Space);
        }
    }
}

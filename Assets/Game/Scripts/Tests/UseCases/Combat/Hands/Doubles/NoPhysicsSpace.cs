using System;
using Bw.Entities.Physics;
using JetBrains.Lifetimes;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Combat.Hands.Doubles
{
    public sealed class NoPhysicsSpace : IPhysicsSpace
    {
        public PhysicsWorld World => default;

        public void MakeOneWay(Lifetime lifetime, PhysicsShape shape) =>
            throw new InvalidOperationException("This space has no world: a test that makes shapes needs a real physics space.");

        public void MakeRider(Lifetime lifetime, PhysicsBody body) =>
            throw new InvalidOperationException("This space has no world: a test that makes riders needs a real physics space.");
    }
}

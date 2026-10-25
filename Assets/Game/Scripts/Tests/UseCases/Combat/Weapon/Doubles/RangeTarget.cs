using Bw.Entities.Physics;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Combat.Weapon.Doubles
{
    public sealed class RangeTarget
    {
        public FakeMortal Mortal { get; }
        public PhysicsBody Body { get; }
        public BodyBox Box { get; }

        public RangeTarget(FakeMortal mortal, PhysicsBody body, BodyBox box)
        {
            Mortal = mortal;
            Body = body;
            Box = box;
        }

        public void MoveTo(Vector2 position)
        {
            var body = Body;
            body.position = position;
            Box.transform.position = position;
        }
    }
}

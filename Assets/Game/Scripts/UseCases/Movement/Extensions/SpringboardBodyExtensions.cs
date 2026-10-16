using Bw.UseCases.Movement.Physics.Abstractions;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Extensions
{
    public static class SpringboardBodyExtensions
    {
        private const float Upward = 0.7f;

        public static bool TryGetLaunchSpeed(this PhysicsBody body, out float speed)
        {
            var begun = body.world.contactBeginEvents;
            for (var index = 0; index < begun.Length; index++)
            {
                var begin = begun[index];
                var mine = begin.shapeA.body.Equals(body);
                if (!mine && !begin.shapeB.body.Equals(body))
                    continue;

                var other = mine ? begin.shapeB : begin.shapeA;
                if (other.userData.objectValue is not ISpringboard springboard || !begin.contactId.isValid)
                    continue;

                var normal = begin.contactId.contact.manifold.normal;
                if ((mine ? -normal.y : normal.y) < Upward)
                    continue;

                speed = Mathf.Sqrt(-2f * Physics2D.gravity.y * body.gravityScale * springboard.LaunchHeight);
                return true;
            }

            speed = 0f;
            return false;
        }
    }
}

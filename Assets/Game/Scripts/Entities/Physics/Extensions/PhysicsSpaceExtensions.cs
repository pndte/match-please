using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Entities.Physics.Extensions
{
    public static class PhysicsSpaceExtensions
    {
        public static PhysicsBody CreateBody(
            this IPhysicsSpace space,
            Lifetime lifetime,
            BoxCollider2D box,
            PhysicsBody.BodyType type)
        {
            var body = PhysicsBodyDefinition.defaultDefinition;
            body.type = type;
            return space.CreateBody(lifetime, box, body, PhysicsShapeDefinition.defaultDefinition);
        }

        public static PhysicsBody CreateBody(
            this IPhysicsSpace space,
            Lifetime lifetime,
            BoxCollider2D box,
            PhysicsBodyDefinition body,
            PhysicsShapeDefinition shape)
        {
            var transform = box.transform;
            body.position = transform.position;
            body.rotation = new PhysicsRotate(transform.eulerAngles.z * Mathf.Deg2Rad);
            var created = space.World.CreateBody(body);

            shape.contactFilter = new PhysicsShape.ContactFilter(box.Categories(), shape.contactFilter.contacts, 0);
            var createdShape = created.CreateShape(box.LocalGeometry(), shape, box.gameObject);
            if (box.IsOneWay())
                space.MakeOneWay(lifetime, createdShape);

            lifetime.OnTermination(() =>
            {
                if (created.isValid)
                    created.Destroy(0);
            });
            return created;
        }
    }
}

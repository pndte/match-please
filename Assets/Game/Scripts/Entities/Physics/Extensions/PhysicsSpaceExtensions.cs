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
            var created = space.CreateBody(lifetime, box.transform, Definition(type));
            var shape = Filtered(PhysicsShapeDefinition.defaultDefinition, box.Categories());
            var createdShape = created.CreateShape(box.LocalGeometry(), shape, box.gameObject);
            if (box.IsOneWay())
                space.MakeOneWay(lifetime, createdShape);

            return created;
        }

        public static PhysicsBody CreateBody(
            this IPhysicsSpace space,
            Lifetime lifetime,
            BodyBox box,
            PhysicsBody.BodyType type) =>
            space.CreateBody(lifetime, box, Definition(type), PhysicsShapeDefinition.defaultDefinition);

        public static PhysicsBody CreateBody(
            this IPhysicsSpace space,
            Lifetime lifetime,
            BodyBox box,
            PhysicsBodyDefinition body,
            PhysicsShapeDefinition shape)
        {
            var created = space.CreateBody(lifetime, box.transform, body);
            created.CreateShape(box.LocalGeometry(), Filtered(shape, box.Categories()), box.gameObject);
            return created;
        }

        private static PhysicsBody CreateBody(
            this IPhysicsSpace space,
            Lifetime lifetime,
            Transform transform,
            PhysicsBodyDefinition body)
        {
            body.position = transform.position;
            body.rotation = new PhysicsRotate(transform.eulerAngles.z * Mathf.Deg2Rad);
            var created = space.World.CreateBody(body);
            lifetime.OnTermination(() =>
            {
                if (created.isValid)
                    created.Destroy(0);
            });
            return created;
        }

        private static PhysicsBodyDefinition Definition(PhysicsBody.BodyType type)
        {
            var body = PhysicsBodyDefinition.defaultDefinition;
            body.type = type;
            return body;
        }

        private static PhysicsShapeDefinition Filtered(PhysicsShapeDefinition shape, PhysicsMask categories)
        {
            shape.contactFilter = new PhysicsShape.ContactFilter(categories, shape.contactFilter.contacts, 0);
            return shape;
        }
    }
}

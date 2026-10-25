using System;
using System.Collections.Generic;
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
            space.CreateShape(lifetime, created, box, shape);
            return created;
        }

        public static PhysicsBody CreateBody(
            this IPhysicsSpace space,
            Lifetime lifetime,
            IReadOnlyList<BodyBox> boxes,
            PhysicsBody.BodyType type)
        {
            var transform = boxes[0].transform;
            var created = space.CreateBody(lifetime, transform, Definition(type));
            for (var index = 0; index < boxes.Count; index++)
            {
                if (boxes[index].transform != transform)
                    throw new InvalidOperationException($"Box '{boxes[index].name}' is not on '{transform.name}': the boxes of one body share its transform.");

                space.CreateShape(lifetime, created, boxes[index], PhysicsShapeDefinition.defaultDefinition);
            }

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

        private static void CreateShape(
            this IPhysicsSpace space,
            Lifetime lifetime,
            PhysicsBody body,
            BodyBox box,
            PhysicsShapeDefinition shape)
        {
            var created = body.CreateShape(box.LocalGeometry(), Filtered(shape, box.Categories()), box.gameObject);
            if (box.IsOneWay)
                space.MakeOneWay(lifetime, created);
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

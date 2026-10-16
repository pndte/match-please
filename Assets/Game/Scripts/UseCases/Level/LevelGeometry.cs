using System;
using System.Collections.Generic;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Level
{
    public sealed class LevelGeometry
    {
        public int ShapeCount => _body.shapeCount;

        private readonly PhysicsBody _body;

        private LevelGeometry(PhysicsBody body)
        {
            _body = body;
        }

        public static LevelGeometry Create(Lifetime lifetime, IPhysicsSpace space, IReadOnlyList<Collider2D> colliders)
        {
            var boxes = StaticBoxes(colliders);
            if (boxes.Count == 0)
                throw new InvalidOperationException("The level has no static colliders: its physics is built from the scene's enabled, non-trigger BoxCollider2D without a Rigidbody2D.");

            var body = space.World.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Static });
            foreach (var box in boxes)
            {
                var shape = body.CreateShape(box.WorldGeometry(), DefinitionOf(box));
                if (box.IsOneWay())
                    space.MakeOneWay(lifetime, shape);

                if (box.TryGetComponent(out Springboard springboard))
                {
                    shape.userData = new PhysicsUserData { objectValue = springboard };
                    shape.contactEvents = true;
                }
            }

            lifetime.OnTermination(() => body.Destroy(0));
            return new LevelGeometry(body);
        }

        private static List<BoxCollider2D> StaticBoxes(IReadOnlyList<Collider2D> colliders)
        {
            var boxes = new List<BoxCollider2D>();
            foreach (var collider in colliders)
            {
                if (!collider.enabled || collider.isTrigger || collider.attachedRigidbody != null)
                    continue;

                if (collider is not BoxCollider2D box)
                    throw new InvalidOperationException($"Level collider '{collider.name}' is a {collider.GetType().Name}: the level's physics is built from BoxCollider2D only.");

                boxes.Add(box);
            }

            boxes.Sort(ByPlace);
            return boxes;
        }

        private static PhysicsShapeDefinition DefinitionOf(BoxCollider2D box)
        {
            var definition = PhysicsShapeDefinition.defaultDefinition;
            definition.contactFilter = new PhysicsShape.ContactFilter(box.Categories(), PhysicsMask.All, 0);
            return definition;
        }

        private static int ByPlace(BoxCollider2D first, BoxCollider2D second)
        {
            var a = (Vector2)first.transform.TransformPoint(first.offset);
            var b = (Vector2)second.transform.TransformPoint(second.offset);
            if (a.x != b.x)
                return a.x.CompareTo(b.x);

            if (a.y != b.y)
                return a.y.CompareTo(b.y);

            var sizeA = first.WorldSize();
            var sizeB = second.WorldSize();
            return sizeA.x != sizeB.x ? sizeA.x.CompareTo(sizeB.x) : sizeA.y.CompareTo(sizeB.y);
        }
    }
}

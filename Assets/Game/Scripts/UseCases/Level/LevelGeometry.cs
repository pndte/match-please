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

        public static LevelGeometry Create(Lifetime lifetime, IPhysicsSpace space, IReadOnlyList<BodyBox> boxes)
        {
            if (boxes.Count == 0)
                throw new InvalidOperationException("The level has no boxes: its physics is built from the scene's BodyBoxes that no object context claims.");

            var body = space.World.CreateBody(new PhysicsBodyDefinition { type = PhysicsBody.BodyType.Static });
            foreach (var box in Sorted(boxes))
            {
                var shape = body.CreateShape(box.WorldGeometry(), DefinitionOf(box), box.gameObject);
                if (box.IsOneWay)
                    space.MakeOneWay(lifetime, shape);
            }

            lifetime.OnTermination(() => body.Destroy(0));
            return new LevelGeometry(body);
        }

        private static List<BodyBox> Sorted(IReadOnlyList<BodyBox> boxes)
        {
            var sorted = new List<BodyBox>(boxes);
            sorted.Sort(ByPlace);
            return sorted;
        }

        private static PhysicsShapeDefinition DefinitionOf(BodyBox box)
        {
            var definition = PhysicsShapeDefinition.defaultDefinition;
            definition.contactFilter = new PhysicsShape.ContactFilter(box.Categories(), PhysicsMask.All, 0);
            definition.contactEvents = box.TryGetComponent<Springboard>(out _);
            return definition;
        }

        private static int ByPlace(BodyBox first, BodyBox second)
        {
            var a = (Vector2)first.transform.TransformPoint(first.Offset);
            var b = (Vector2)second.transform.TransformPoint(second.Offset);
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

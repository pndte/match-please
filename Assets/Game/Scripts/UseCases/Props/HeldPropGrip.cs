using System;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Props
{
    public sealed class HeldPropGrip
    {
        private readonly Vector2 _size;
        private readonly GameObject _owner;
        private readonly PhysicsWorld _world;
        private readonly PhysicsShapeDefinition _definition;

        private PhysicsBody _body;

        public HeldPropGrip(BodyBox shape, IPhysicsSpace space, int layer)
        {
            _size = shape.WorldSize();
            _owner = shape.gameObject;
            _world = space.World;
            _definition = Definition(layer);
        }

        public void Enter(Lifetime lifetime)
        {
            if (_body.isValid)
                throw new InvalidOperationException("A held prop's grip enters the world once at a time: it is already there.");

            var definition = PhysicsBodyDefinition.defaultDefinition;
            definition.type = PhysicsBody.BodyType.Kinematic;
            var body = _world.CreateBody(definition);
            body.CreateShape(PolygonGeometry.CreateBox(_size, 0f, new PhysicsTransform(Vector2.zero), false), _definition, _owner);
            _body = body;
            lifetime.OnTermination(() =>
            {
                if (body.isValid)
                    body.Destroy(0);

                _body = default;
            });
        }

        public void Place(Vector2 center, float angle)
        {
            if (!_body.isValid)
                throw new InvalidOperationException("A held prop's grip is placed only while it is in the world.");

            _body.transform = new PhysicsTransform(center, new PhysicsRotate(angle * Mathf.Deg2Rad));
        }

        private static PhysicsShapeDefinition Definition(int layer)
        {
            var categories = PhysicsMask.None;
            categories.SetBit(layer);
            var definition = PhysicsShapeDefinition.defaultDefinition;
            definition.density = 0f;
            definition.contactFilter = new PhysicsShape.ContactFilter(categories, PhysicsMask.All, 0);
            return definition;
        }
    }
}

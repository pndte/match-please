using System;
using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Hands.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Combat.Hands
{
    public sealed class HandsGrip
    {
        public GripPose Pose { get; private set; }

        private readonly PhysicsWorld _world;

        private PhysicsBody _body;

        public HandsGrip(IPhysicsSpace space)
        {
            _world = space.World;
        }

        public void Enter(Lifetime lifetime, IHeldBox box)
        {
            if (_body.isValid)
                throw new InvalidOperationException("The hands' grip enters the world once at a time: it is already there.");

            var definition = PhysicsBodyDefinition.defaultDefinition;
            definition.type = PhysicsBody.BodyType.Kinematic;
            var body = _world.CreateBody(definition);
            body.CreateShape(PolygonGeometry.CreateBox(box.Box.WorldSize(), 0f, new PhysicsTransform(Vector2.zero), false), Definition(box.Layer), box.Box.gameObject);
            _body = body;
            Pose = default;
            lifetime.OnTermination(() =>
            {
                if (body.isValid)
                    body.Destroy(0);

                _body = default;
            });
        }

        public void Place(GripPose pose)
        {
            if (!_body.isValid)
                throw new InvalidOperationException("The hands' grip is placed only while it is in the world.");

            Pose = pose;
            _body.transform = new PhysicsTransform(pose.Center, new PhysicsRotate(pose.Angle * Mathf.Deg2Rad));
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

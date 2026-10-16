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
        private readonly PhysicsShapeDefinition _definition;

        private PhysicsShape _shape;

        public HeldPropGrip(BodyBox shape, int layer)
        {
            _size = shape.WorldSize();
            _owner = shape.gameObject;
            _definition = Definition(layer);
        }

        public void Attach(Lifetime lifetime, PhysicsBody holder)
        {
            if (_shape.isValid)
                throw new InvalidOperationException("A held prop grips one holder at a time: its grip is already on a holder's body.");

            var shape = holder.CreateShape(BoxAt(Vector2.zero), _definition, _owner);
            _shape = shape;
            lifetime.OnTermination(() =>
            {
                if (shape.isValid)
                    shape.Destroy();

                _shape = default;
            });
        }

        public void Place(Vector2 center)
        {
            if (!_shape.isValid)
                throw new InvalidOperationException("A held prop's grip is placed only while it is on a holder's body.");

            _shape.polygonGeometry = BoxAt(_shape.body.transform.InverseTransformPoint(center));
        }

        private PolygonGeometry BoxAt(Vector2 local) =>
            PolygonGeometry.CreateBox(_size, 0f, new PhysicsTransform(local), false);

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

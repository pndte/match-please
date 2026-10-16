using Bw.Entities.Physics;
using Bw.Entities.Physics.Extensions;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Extensions
{
    public static class DynamicBodyConfigExtensions
    {
        public static PhysicsBody CreateBody(
            this DynamicBodyConfig config,
            IPhysicsSpace space,
            Lifetime lifetime,
            BoxCollider2D box,
            PhysicsBody.BodyType type)
        {
            var definition = config.BodyDefinition();
            definition.type = type;
            var body = space.CreateBody(lifetime, box, definition, config.ShapeDefinition());
            body.SweepContinuously(box.WorldOffset());
            return body;
        }

        public static PhysicsBodyDefinition BodyDefinition(this DynamicBodyConfig config)
        {
            var definition = PhysicsBodyDefinition.defaultDefinition;
            definition.type = PhysicsBody.BodyType.Dynamic;
            definition.gravityScale = config.GravityScale;
            definition.linearDamping = config.LinearDamping;
            definition.angularDamping = config.AngularDamping;
            return definition;
        }

        public static PhysicsShapeDefinition ShapeDefinition(this DynamicBodyConfig config)
        {
            var definition = PhysicsShapeDefinition.defaultDefinition;
            definition.density = config.Density;
            definition.preSolveCallbacks = true;
            definition.contactFilter = new PhysicsShape.ContactFilter(definition.contactFilter.categories, config.Contacts.AsPhysicsMask(), 0);
            var material = definition.surfaceMaterial;
            material.friction = config.Friction;
            material.bounciness = config.Bounciness;
            definition.surfaceMaterial = material;
            return definition;
        }
    }
}

using System;
using Bw.Entities.Physics.Extensions;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Mortality.Extensions
{
    public sealed class PhysicsShapeContactExtensionsTests
    {
        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private TestSpace _space;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _space = new TestSpace(_lifetime.Lifetime, _objects);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
        }

        [Test]
        public void ContactOnABoxIsThePointInItsObjectsFrame()
        {
            var shape = OnlyShapeOf(_space.Body(_space.Box("crate", new Vector2(3f, 2f), new Vector2(1f, 2f), new Vector2(0f, 0.5f))));

            var contact = shape.ContactAt(new Vector2(3.4f, 2.9f));

            AssertNear(contact.Point, new Vector2(0.4f, 0.9f));
        }

        [Test]
        public void ContactOnATurnedBoxIsUnturned()
        {
            var box = _space.Box("crate", new Vector2(3f, 2f), new Vector2(1f, 2f), new Vector2(0f, 0.5f));
            box.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            var shape = OnlyShapeOf(_space.Body(box));

            var contact = shape.ContactAt(new Vector2(2.1f, 2.4f));

            AssertNear(contact.Point, new Vector2(0.4f, 0.9f));
        }

        [Test]
        public void ContactOnAGripCarriedByAnotherBodyIsInTheGrippedObjectsFrame()
        {
            var holder = _space.Body(_space.Box("holder", Vector2.zero, Vector2.one, Vector2.zero));
            var crate = _space.Box("crate", new Vector2(9f, 9f), Vector2.one, new Vector2(0f, 0.5f));
            var grip = holder.CreateShape(Square(new Vector2(1f, 0.5f), 0f), PhysicsShapeDefinition.defaultDefinition, crate.gameObject);

            var contact = grip.ContactAt(new Vector2(1.2f, 0.9f));

            AssertNear(contact.Point, new Vector2(0.2f, 0.9f));
        }

        [Test]
        public void ContactOnATurnedGripIsInTheGrippedObjectsTurnedFrame()
        {
            var holder = _space.Body(_space.Box("holder", Vector2.zero, Vector2.one, Vector2.zero));
            var crate = _space.Box("crate", new Vector2(9f, 9f), Vector2.one, new Vector2(0f, 0.5f));
            var grip = holder.CreateShape(Square(new Vector2(1f, 0.5f), 90f), PhysicsShapeDefinition.defaultDefinition, crate.gameObject);

            var contact = grip.ContactAt(new Vector2(0.6f, 0.7f));

            AssertNear(contact.Point, new Vector2(0.2f, 0.9f));
        }

        [Test]
        public void ContactOnARoundShapeThrows()
        {
            var body = _space.Body(_space.Box("crate", Vector2.zero, Vector2.one, Vector2.zero));
            var circle = body.CreateShape(new CircleGeometry { center = Vector2.zero, radius = 0.5f }, PhysicsShapeDefinition.defaultDefinition);

            Assert.Throws<InvalidOperationException>(() => circle.ContactAt(Vector2.zero));
        }

        [Test]
        public void ContactOnABoxOfAnObjectWithoutABodyBoxThrows()
        {
            var body = _space.Body(_space.Box("crate", Vector2.zero, Vector2.one, Vector2.zero));
            var plain = _objects.Create("plain");
            var shape = body.CreateShape(Square(Vector2.zero, 0f), PhysicsShapeDefinition.defaultDefinition, plain);

            Assert.Throws<InvalidOperationException>(() => shape.ContactAt(Vector2.zero));
        }

        [Test]
        public void ContactOnABoxWithoutAnOwnerThrows()
        {
            var body = _space.Body(_space.Box("crate", Vector2.zero, Vector2.one, Vector2.zero));
            var shape = body.CreateShape(Square(Vector2.zero, 0f), PhysicsShapeDefinition.defaultDefinition);

            Assert.Throws<InvalidOperationException>(() => shape.ContactAt(Vector2.zero));
        }

        private static PolygonGeometry Square(Vector2 center, float degrees) =>
            PolygonGeometry.CreateBox(Vector2.one, 0f, new PhysicsTransform(center, new PhysicsRotate(degrees * Mathf.Deg2Rad)), false);

        private static PhysicsShape OnlyShapeOf(PhysicsBody body)
        {
            using var shapes = body.GetShapes(Allocator.Temp);
            return shapes[0];
        }

        private static void AssertNear(Vector2 actual, Vector2 expected)
        {
            Assert.That(actual.x, Is.EqualTo(expected.x).Within(1e-4f));
            Assert.That(actual.y, Is.EqualTo(expected.y).Within(1e-4f));
        }
    }
}

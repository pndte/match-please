using Bw.Entities.Physics.Extensions;
using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Mortality.Doubles;
using Bw.UseCases.Mortality;
using JetBrains.Lifetimes;
using NUnit.Framework;
using Unity.Collections;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Mortality
{
    public sealed class CorpseHitboxTests
    {
        private const int CharacterLayer = 8;
        private const int CorpseLayer = 2;

        private LifetimeDefinition _lifetime;
        private HiddenObjects _objects;
        private MortalFixture _fixture;
        private PhysicsBody _body;

        [SetUp]
        public void SetUp()
        {
            _lifetime = new LifetimeDefinition();
            _objects = new HiddenObjects();
            _fixture = new MortalFixture();
            var space = new TestSpace(_lifetime.Lifetime, _objects);
            _body = space.Body(space.Box("body", Vector2.zero, new Vector2(1f, 2f), new Vector2(0f, 1f), CharacterLayer), PhysicsBody.BodyType.Kinematic);
        }

        [TearDown]
        public void TearDown()
        {
            _lifetime.Terminate();
            _objects.Dispose();
            _fixture.Dispose();
        }

        [Test]
        public void LivingBodyKeepsItsLayer()
        {
            _ = new CorpseHitbox(_lifetime.Lifetime, _fixture.Mortal, _body, CorpseLayer);

            Assert.That(LayerOfTheBody(), Is.EqualTo(CharacterLayer));
        }

        [Test]
        public void DeathMovesTheBodyToTheCorpseLayer()
        {
            _ = new CorpseHitbox(_lifetime.Lifetime, _fixture.Mortal, _body, CorpseLayer);

            _fixture.Mortal.Apply(VitalsOf.Dead(0f, -100f));

            Assert.That(LayerOfTheBody(), Is.EqualTo(CorpseLayer));
        }

        [Test]
        public void BodyOfTheAlreadyDeadMovesAtOnce()
        {
            _fixture.Mortal.Apply(VitalsOf.Dead());

            _ = new CorpseHitbox(_lifetime.Lifetime, _fixture.Mortal, _body, CorpseLayer);

            Assert.That(LayerOfTheBody(), Is.EqualTo(CorpseLayer));
        }

        private int LayerOfTheBody()
        {
            using var shapes = _body.GetShapes(Allocator.Temp);
            return shapes[0].Layer();
        }
    }
}

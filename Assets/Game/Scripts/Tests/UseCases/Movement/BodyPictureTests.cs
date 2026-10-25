using Bw.Entities.Physics;
using Bw.Tests.Entities.Physics.Doubles;
using Bw.UseCases.Movement;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement
{
    public sealed class BodyPictureTests
    {
        private PhysicsTestWorld _physics;
        private BodyBox _box;
        private BodyPicture _picture;

        [SetUp]
        public void SetUp()
        {
            _physics = new PhysicsTestWorld();
            _box = _physics.Box("Character", new Vector2(1f, 2f), new Vector2(1f, 2f));
            _box.transform.position = new Vector3(1f, 2f, 5f);
            _picture = new BodyPicture(_box);
        }

        [TearDown]
        public void TearDown() =>
            _physics.Dispose();

        [Test]
        public void PositionIsWhereThePictureStands()
        {
            _box.transform.position = new Vector3(-3f, 4f, 5f);

            Assert.That(_picture.Position, Is.EqualTo(new Vector2(-3f, 4f)));
        }

        [Test]
        public void PlaceMovesThePictureAndKeepsItsDepth()
        {
            _picture.Place(new Vector2(7f, -1f));

            Assert.That(_box.transform.position, Is.EqualTo(new Vector3(7f, -1f, 5f)));
        }
    }
}

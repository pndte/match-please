using Bw.Entities.Physics.Extensions;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Physics.Doubles;
using Bw.Tests.UseCases.Movement.Doubles;
using Bw.UseCases.Level;
using Bw.UseCases.Movement.Physics;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.Tests.UseCases.Movement.Physics
{
    public sealed class ShapeColliderTests
    {
        private const float HalfWidth = 0.5f;
        private const float HalfHeight = 1f;
        private const float NearestStop = 0.01f;
        private const float FarthestStop = 0.03f;

        private PhysicsTestWorld _physics;
        private PhysicsBody _body;
        private MovingPlatformCollection _platforms;
        private ShovableCollection _shovables;
        private ShapeCollider _collider;

        [SetUp]
        public void SetUp()
        {
            _physics = new PhysicsTestWorld();
            var box = _physics.Box("Character", Vector2.zero, new Vector2(2f * HalfWidth, 2f * HalfHeight), Vector2.zero, PhysicsTestWorld.Character);
            _body = _physics.Space.CreateBody(_physics.Lifetime, box, PhysicsBody.BodyType.Kinematic);
            _platforms = new MovingPlatformCollection();
            _shovables = new ShovableCollection();
            _collider = new ShapeCollider(_body, box, 1 << PhysicsTestWorld.Ground, new PlatformRider(box, _platforms, _shovables, _physics.Clock), _shovables);
        }

        [TearDown]
        public void TearDown() =>
            _physics.Dispose();

        [Test]
        public void CastInTheOpenGoesTheWholeWay()
        {
            Assert.That(_collider.Cast(Vector2.zero, Vector2.right, 3f), Is.EqualTo(3f));
        }

        [TestCase(1f, 0f)]
        [TestCase(-1f, 0f)]
        [TestCase(0f, 1f)]
        [TestCase(0f, -1f)]
        public void CastStopsJustShortOfASolid(float x, float y)
        {
            var direction = new Vector2(x, y);
            var gap = 1.5f;
            var reach = new Vector2(HalfWidth, HalfHeight) * direction;
            _physics.Static("Wall", direction * (gap + 1f) + reach, new Vector2(2f, 2f));

            var distance = _collider.Cast(Vector2.zero, direction, 5f);

            Assert.That(distance, Is.InRange(gap - FarthestStop, gap - NearestStop));
        }

        [Test]
        public void CastNeverGoesBack()
        {
            _physics.Static("Wall", new Vector2(HalfWidth + 0.5f, 0f), Vector2.one);

            Assert.That(_collider.Cast(Vector2.zero, Vector2.right, 1f), Is.EqualTo(0f));
        }

        [Test]
        public void CastMovingAwayFromATouchingSolidIsFree()
        {
            _physics.Static("Wall", new Vector2(HalfWidth + 0.5f + 0.005f, 0f), Vector2.one);

            Assert.That(_collider.Cast(Vector2.zero, Vector2.left, 1f), Is.EqualTo(1f));
        }

        [Test]
        public void CastIgnoresShapesOutsideTheGround()
        {
            _physics.Space.CreateBody(_physics.Lifetime, _physics.Box("Other", new Vector2(2f, 0f), Vector2.one, Vector2.zero, PhysicsTestWorld.Character), PhysicsBody.BodyType.Static);

            Assert.That(_collider.Cast(Vector2.zero, Vector2.right, 3f), Is.EqualTo(3f));
        }

        [Test]
        public void CastIgnoresTheOwnBody()
        {
            var box = _physics.Box("Crate", Vector2.zero, Vector2.one);
            var own = _physics.Space.CreateBody(_physics.Lifetime, box, PhysicsBody.BodyType.Kinematic);
            var collider = new ShapeCollider(own, box, 1 << PhysicsTestWorld.Ground, new PlatformRider(box, _platforms, _shovables, _physics.Clock), _shovables);

            Assert.That(collider.Cast(new Vector2(-1f, 0f), Vector2.right, 2f), Is.EqualTo(2f));
        }

        [TestCase(1f, 0f)]
        [TestCase(0f, 1f)]
        [TestCase(0f, -1f)]
        public void CastPassesPlanksEveryWay(float x, float y)
        {
            var direction = new Vector2(x, y);
            _physics.StaticPlank("Plank", direction * 2f, new Vector2(4f, 0.2f));

            Assert.That(_collider.Cast(Vector2.zero, direction, 5f), Is.EqualTo(5f));
        }

        [Test]
        public void FallLandsOnSolidGround()
        {
            _physics.Static("Floor", new Vector2(0f, -HalfHeight - 1f - 0.5f), new Vector2(10f, 1f));

            Assert.That(_collider.Fall(Vector2.zero, 3f), Is.InRange(1f - FarthestStop, 1f - NearestStop));
        }

        [Test]
        public void FallLandsOnAPlankBelowTheFeet()
        {
            _physics.StaticPlank("Plank", new Vector2(0f, -HalfHeight - 0.5f - 0.1f), new Vector2(4f, 0.2f));

            Assert.That(_collider.Fall(Vector2.zero, 3f), Is.InRange(0.5f - FarthestStop, 0.5f - NearestStop));
        }

        [Test]
        public void FallPassesAPlankAlreadyAboveTheFeet()
        {
            _physics.StaticPlank("Plank", new Vector2(0f, -HalfHeight + 0.05f - 0.1f), new Vector2(4f, 0.2f));

            Assert.That(_collider.Fall(Vector2.zero, 3f), Is.EqualTo(3f));
        }

        [Test]
        public void FallStillCatchesAPlankWithinTheSkinAboveTheFeet()
        {
            _physics.StaticPlank("Plank", new Vector2(0f, -HalfHeight + 0.01f - 0.1f), new Vector2(4f, 0.2f));

            Assert.That(_collider.Fall(Vector2.zero, 3f), Is.EqualTo(0f));
        }

        [Test]
        public void ShovablesAheadAreTheRegisteredBodiesInTheWayAtTheCastsGap()
        {
            var crate = new RecordedShovable();
            var crateBody = _physics.Static("Crate", new Vector2(HalfWidth + 1f + 0.5f, 0f), Vector2.one);
            _shovables.Add(crateBody, crate);
            _physics.Static("Wall", new Vector2(-3f, 0f), Vector2.one);

            var contacts = _collider.ShovablesAhead(Vector2.zero, Vector2.right, 2f);

            Assert.That(contacts, Has.Count.EqualTo(1));
            Assert.That(contacts[0].Shovable, Is.SameAs(crate));
            Assert.That(contacts[0].Gap, Is.EqualTo(_collider.Cast(Vector2.zero, Vector2.right, 2f)).Within(1e-5f));
        }

        [Test]
        public void UnregisteredBodiesAndBodiesBehindAreNoShovables()
        {
            _physics.Static("Wall", new Vector2(2f, 0f), Vector2.one);
            var behind = _physics.Static("Behind", new Vector2(-2f, 0f), Vector2.one);
            _shovables.Add(behind, new RecordedShovable());

            Assert.That(_collider.ShovablesAhead(Vector2.zero, Vector2.right, 3f), Is.Empty);
        }

        [Test]
        public void SpringboardRightUnderTheFeetIsALaunch()
        {
            var springboard = _physics.Box("Springboard", new Vector2(0f, -HalfHeight - 0.01f - 0.5f), new Vector2(4f, 1f));
            springboard.gameObject.AddComponent<Springboard>().With("_launchHeight", 7f);
            _physics.Space.CreateBody(_physics.Lifetime, springboard, PhysicsBody.BodyType.Static);

            Assert.That(_collider.TryGetLaunch(Vector2.zero, out var height), Is.True);
            Assert.That(height, Is.EqualTo(7f));
        }

        [Test]
        public void SpringboardOutOfReachOfTheFeetIsNoLaunch()
        {
            var springboard = _physics.Box("Springboard", new Vector2(0f, -HalfHeight - 0.2f - 0.5f), new Vector2(4f, 1f));
            springboard.gameObject.AddComponent<Springboard>().With("_launchHeight", 7f);
            _physics.Space.CreateBody(_physics.Lifetime, springboard, PhysicsBody.BodyType.Static);

            Assert.That(_collider.TryGetLaunch(Vector2.zero, out var height), Is.False);
            Assert.That(height, Is.EqualTo(0f));
        }

        [Test]
        public void PlainFloorIsNoLaunch()
        {
            _physics.Static("Floor", new Vector2(0f, -HalfHeight - 0.01f - 0.5f), new Vector2(4f, 1f));

            Assert.That(_collider.TryGetLaunch(Vector2.zero, out _), Is.False);
        }

        [Test]
        public void GroundedBodyIsCarriedByThePlatformUnderIt()
        {
            _platforms.Add(new MovedPlatform(new Rect(-2f, -HalfHeight - 0.3f + 0.2f, 4f, 0.3f), new Vector2(0.4f, 0.2f)));

            var carried = _collider.Carry(Vector2.zero, true, false);

            Assert.That(carried.x, Is.EqualTo(0.4f).Within(1e-5f));
            Assert.That(carried.y, Is.EqualTo(0.2f + 0.015f).Within(1e-4f));
        }

        [Test]
        public void CarryStopsAtAWall()
        {
            _platforms.Add(new MovedPlatform(new Rect(-2f, -HalfHeight - 0.3f, 4f, 0.3f), new Vector2(1f, 0f)));
            _physics.Static("Wall", new Vector2(HalfWidth + 0.3f + 0.5f, 0f), Vector2.one);

            var carried = _collider.Carry(Vector2.zero, true, false);

            Assert.That(carried.x, Is.InRange(0.3f - FarthestStop, 0.3f - NearestStop));
        }

        [Test]
        public void AirborneBodyIsNotCarried()
        {
            _platforms.Add(new MovedPlatform(new Rect(-2f, -HalfHeight - 0.3f, 4f, 0.3f), new Vector2(1f, 0f)));

            Assert.That(_collider.Carry(Vector2.zero, false, false), Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void OwnBodyIsRecognised()
        {
            var other = _physics.Static("Other", new Vector2(9f, 9f), Vector2.one);

            Assert.That(_collider.Is(_body), Is.True);
            Assert.That(_collider.Is(other), Is.False);
        }
    }
}

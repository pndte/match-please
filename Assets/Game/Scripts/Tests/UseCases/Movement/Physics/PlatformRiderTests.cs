using Bw.Entities.Physics;
using Bw.Tests.Doubles;
using Bw.Tests.Entities.Physics.Doubles;
using Bw.Tests.UseCases.Movement.Doubles;
using Bw.UseCases.Movement.Physics;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement.Physics
{
    public sealed class PlatformRiderTests
    {
        private const float Skin = 0.015f;

        private PhysicsTestWorld _physics;
        private BodyBox _box;
        private MovingPlatformCollection _platforms;
        private ShovableCollection _shovables;
        private PlatformRider _rider;

        [SetUp]
        public void SetUp()
        {
            _physics = new PhysicsTestWorld();
            _box = _physics.Objects.Create("Character").AddComponent<BodyBox>().With("_size", new Vector2(1f, 2f)).With("_offset", new Vector2(0f, 1f));
            _platforms = new MovingPlatformCollection();
            _shovables = new ShovableCollection();
            _rider = new PlatformRider(_box, _platforms, _shovables, _physics.Clock);
        }

        [TearDown]
        public void TearDown() =>
            _physics.Dispose();

        [Test]
        public void NoPlatformsCarryNothing()
        {
            Assert.That(_rider.TryGetRidden(Vector2.zero, out var shift), Is.False);
            Assert.That(shift, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void FeetOnThePlatformsTopBeforeItMovedRideItsShift()
        {
            _platforms.Add(Platform(topBefore: 0f, shift: new Vector2(0.5f, 0.3f)));

            Assert.That(_rider.TryGetRidden(Vector2.zero, out var shift), Is.True);
            Assert.That(shift, Is.EqualTo(new Vector2(0.5f, 0.3f)));
        }

        [TestCase(0.04f, true)]
        [TestCase(-0.04f, true)]
        [TestCase(0.06f, false)]
        [TestCase(-0.06f, false)]
        public void FeetRideWithinTheToleranceOfTheTop(float above, bool riding)
        {
            _platforms.Add(Platform(topBefore: -above, shift: Vector2.zero));

            Assert.That(_rider.TryGetRidden(Vector2.zero, out _), Is.EqualTo(riding));
        }

        [Test]
        public void PlatformBesideTheFeetCarriesNothing()
        {
            _platforms.Add(new MovedPlatform(new Rect(0.5f, -0.3f, 2f, 0.3f), Vector2.zero));

            Assert.That(_rider.TryGetRidden(Vector2.zero, out _), Is.False);
        }

        [Test]
        public void HighestPlatformUnderTheFeetWins()
        {
            _platforms.Add(Platform(topBefore: -0.03f, shift: Vector2.left));
            _platforms.Add(Platform(topBefore: 0.02f, shift: Vector2.right));
            _platforms.Add(Platform(topBefore: 0.01f, shift: Vector2.up));

            Assert.That(_rider.TryGetRidden(Vector2.zero, out var shift), Is.True);
            Assert.That(shift, Is.EqualTo(Vector2.right));
        }

        [Test]
        public void BoxOffsetPlacesTheFeet()
        {
            _platforms.Add(Platform(topBefore: 5f, shift: Vector2.zero));

            Assert.That(_rider.TryGetRidden(new Vector2(0f, 5f), out _), Is.True);
        }

        [Test]
        public void CrateUnderTheFeetIsRiddenLikeAPlatform()
        {
            _shovables.Add(Crate(top: 0f, Vector2.zero), new RecordedShovable());

            Assert.That(_rider.TryGetRidden(Vector2.zero, out var shift), Is.True);
            Assert.That(shift, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void MovingCrateCarriesTheFeetByItsStep()
        {
            var crate = Crate(top: 0f, new Vector2(3f, 0f));
            _shovables.Add(crate, new RecordedShovable());

            Assert.That(_rider.TryGetRidden(new Vector2(3f * _physics.Clock.Duration, 0f), out var shift), Is.True);
            Assert.That(shift.x, Is.EqualTo(3f * _physics.Clock.Duration).Within(1e-5f));
        }

        [Test]
        public void RisingPlatformThatPassedTheFeetLiftsThemOntoItsTop()
        {
            _platforms.Add(Platform(topBefore: -0.1f, shift: new Vector2(0f, 0.2f)));

            Assert.That(_rider.Scooped(Vector2.zero, Skin, false).y, Is.EqualTo(0.1f + Skin).Within(1e-5f));
        }

        [Test]
        public void ScoopTakesTheHighestRisingPlatform()
        {
            _platforms.Add(Platform(topBefore: -0.1f, shift: new Vector2(0f, 0.2f)));
            _platforms.Add(Platform(topBefore: -0.05f, shift: new Vector2(0f, 0.2f)));

            Assert.That(_rider.Scooped(Vector2.zero, Skin, false).y, Is.EqualTo(0.15f + Skin).Within(1e-5f));
        }

        [Test]
        public void RisingPlankLiftsTheFeetUnlessTheyDropThroughIt()
        {
            var plank = Platform(topBefore: -0.1f, shift: new Vector2(0f, 0.2f));
            plank.OneWay = true;
            _platforms.Add(plank);

            Assert.That(_rider.Scooped(Vector2.zero, Skin, false).y, Is.EqualTo(0.1f + Skin).Within(1e-5f));
            Assert.That(_rider.Scooped(Vector2.zero, Skin, true), Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void DroppingStillRidesASolidPlatform()
        {
            _platforms.Add(Platform(topBefore: -0.1f, shift: new Vector2(0f, 0.2f)));

            Assert.That(_rider.Scooped(Vector2.zero, Skin, true).y, Is.EqualTo(0.1f + Skin).Within(1e-5f));
        }

        [Test]
        public void SinkingPlatformLiftsNothing()
        {
            _platforms.Add(Platform(topBefore: 0.1f, shift: new Vector2(0f, -0.2f)));

            Assert.That(_rider.Scooped(Vector2.zero, Skin, false), Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void RisingPlatformThatStartedAboveTheFeetLiftsNothing()
        {
            _platforms.Add(Platform(topBefore: 0.5f, shift: new Vector2(0f, 0.2f)));

            Assert.That(_rider.Scooped(Vector2.zero, Skin, false), Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void FeetAlreadyOverTheTopAreNotLifted()
        {
            _platforms.Add(Platform(topBefore: -0.5f, shift: new Vector2(0f, 0.2f)));

            Assert.That(_rider.Scooped(Vector2.zero, Skin, false), Is.EqualTo(Vector2.zero));
        }

        private static MovedPlatform Platform(float topBefore, Vector2 shift) =>
            new(new Rect(-2f + shift.x, topBefore + shift.y - 0.3f, 4f, 0.3f), shift);

        private UnityEngine.LowLevelPhysics2D.PhysicsBody Crate(float top, Vector2 velocity)
        {
            var shift = velocity * _physics.Clock.Duration;
            var body = _physics.Dynamic("Crate", new Vector2(shift.x, top - 0.25f + shift.y), new Vector2(2f, 0.5f));
            body.linearVelocity = velocity;
            return body;
        }
    }
}

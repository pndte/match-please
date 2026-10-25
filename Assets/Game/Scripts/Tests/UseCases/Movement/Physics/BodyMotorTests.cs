using Bw.Tests.Doubles;
using Bw.Tests.UseCases.Movement.Doubles;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Physics;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement.Physics
{
    public sealed class BodyMotorTests
    {
        private const float Duration = 0.1f;

        private ScriptedCollider _collider;
        private FixedBodyConfig _config;
        private BodyMotor _motor;

        private static float Gravity => Physics2D.gravity.y;

        [SetUp]
        public void SetUp()
        {
            _collider = new ScriptedCollider();
            _config = new FixedBodyConfig();
            _motor = new BodyMotor(_collider, new ManualClock(Duration), _config);
        }

        [Test]
        public void BodyInTheOpenFallsUnderGravity()
        {
            var state = _motor.Step(Airborne(new Vector2(0f, 10f)), Drive(Vector2.zero), out var launched);

            Assert.That(state.Velocity.y, Is.EqualTo(Gravity * Duration).Within(1e-5f));
            Assert.That(state.Position.y, Is.EqualTo(10f + Gravity * Duration * Duration).Within(1e-5f));
            Assert.That(state.Grounded, Is.False);
            Assert.That(launched, Is.False);
        }

        [Test]
        public void DriveGravityScaleAndConfigGravityScaleMultiply()
        {
            _config.GravityScale = 2f;
            var motor = new BodyMotor(_collider, new ManualClock(Duration), _config);

            var state = motor.Step(Airborne(Vector2.zero), Drive(Vector2.zero, 1.5f), out _);

            Assert.That(state.Velocity.y, Is.EqualTo(Gravity * 3f * Duration).Within(1e-5f));
        }

        [Test]
        public void BodyThatReachesTheFloorLandsOnItAndStops()
        {
            _collider.Floor = 9.95f;

            var state = _motor.Step(Airborne(new Vector2(0f, 10f)), Drive(Vector2.zero), out _);

            Assert.That(state.Position.y, Is.EqualTo(9.95f).Within(1e-5f));
            Assert.That(state.Velocity.y, Is.EqualTo(0f));
            Assert.That(state.Grounded, Is.True);
        }

        [Test]
        public void FallLandsOnAPlank()
        {
            _collider.Plank = 9.95f;

            var state = _motor.Step(Airborne(new Vector2(0f, 10f)), Drive(Vector2.zero), out _);

            Assert.That(state.Grounded, Is.True);
            Assert.That(_collider.Calls, Has.Member("Fall"));
        }

        [Test]
        public void DroppingBodyPassesAPlank()
        {
            _collider.Plank = 9.95f;

            var state = _motor.Step(Airborne(new Vector2(0f, 10f)), new BodyDrive(Vector2.zero, 1f, true), out _);

            Assert.That(state.Grounded, Is.False);
            Assert.That(state.Position.y, Is.LessThan(9.95f));
            Assert.That(_collider.Calls, Has.Member("Cast down"));
        }

        [Test]
        public void DroppingBodyStillLandsOnSolidGround()
        {
            _collider.Floor = 9.95f;

            var state = _motor.Step(Airborne(new Vector2(0f, 10f)), new BodyDrive(Vector2.zero, 1f, true), out _);

            Assert.That(state.Grounded, Is.True);
            Assert.That(state.Position.y, Is.EqualTo(9.95f).Within(1e-5f));
        }

        [Test]
        public void CeilingStopsTheRiseWithoutGrounding()
        {
            _collider.Ceiling = 0.2f;

            var state = _motor.Step(Airborne(Vector2.zero), Drive(new Vector2(0f, 10f)), out _);

            Assert.That(state.Position.y, Is.EqualTo(0.2f).Within(1e-5f));
            Assert.That(state.Velocity.y, Is.EqualTo(0f));
            Assert.That(state.Grounded, Is.False);
        }

        [Test]
        public void WalkMovesByTheDriveUntilAWall()
        {
            _collider.Right = 0.3f;

            var open = _motor.Step(Airborne(new Vector2(-5f, 0f)), Drive(new Vector2(2f, 0f)), out _);
            var blocked = _motor.Step(Airborne(Vector2.zero), Drive(new Vector2(10f, 0f)), out _);

            Assert.That(open.Position.x, Is.EqualTo(-4.8f).Within(1e-5f));
            Assert.That(blocked.Position.x, Is.EqualTo(0.3f).Within(1e-5f));
        }

        [Test]
        public void PushCarriesTheBodyAndFadesByTheAirDeceleration()
        {
            var state = _motor.Step(Pushed(5f, false), Drive(Vector2.zero), out _);

            Assert.That(state.Position.x, Is.EqualTo(0.5f).Within(1e-5f));
            Assert.That(state.HorizontalPush, Is.EqualTo(5f - _config.AirPushDeceleration * Duration).Within(1e-5f));
        }

        [Test]
        public void PushOfAGroundedBodyFadesByTheGroundDeceleration()
        {
            _collider.Floor = 0f;

            var state = _motor.Step(Pushed(5f, true), Drive(Vector2.zero), out _);

            Assert.That(state.HorizontalPush, Is.EqualTo(5f - _config.PushDeceleration * Duration).Within(1e-5f));
        }

        [Test]
        public void PushIntoAWallIsSpent()
        {
            _collider.Right = 0.2f;

            var state = _motor.Step(Pushed(5f, false), Drive(Vector2.zero), out _);

            Assert.That(state.Position.x, Is.EqualTo(0.2f).Within(1e-5f));
            Assert.That(state.HorizontalPush, Is.EqualTo(0f));
        }

        [Test]
        public void WalkIntoAWallAgainstThePushKeepsThePush()
        {
            _collider.Left = -0.1f;

            var state = _motor.Step(Pushed(5f, false), Drive(new Vector2(-10f, 0f)), out _);

            Assert.That(state.Position.x, Is.EqualTo(-0.1f).Within(1e-5f));
            Assert.That(state.HorizontalPush, Is.EqualTo(4.5f).Within(1e-5f));
        }

        [Test]
        public void GroundedBodyIsCarriedBeforeItMoves()
        {
            _collider.Floor = 0f;
            _collider.CarryShift = new Vector2(0.5f, 0f);

            var state = _motor.Step(Grounded(Vector2.zero), Drive(new Vector2(1f, 0f)), out _);

            Assert.That(_collider.Calls[0], Is.EqualTo("Carry True False"));
            Assert.That(state.Position.x, Is.EqualTo(0.6f).Within(1e-5f));
        }

        [Test]
        public void DroppingBodyIsCarriedAsDropping()
        {
            _collider.Floor = 0f;

            _motor.Step(Grounded(Vector2.zero), new BodyDrive(Vector2.zero, 1f, true), out _);

            Assert.That(_collider.Calls[0], Is.EqualTo("Carry True True"));
        }

        [Test]
        public void GroundedBodyOnASpringboardLaunchesToItsHeight()
        {
            _collider.LaunchHeight = 2f;
            var motor = new BodyMotor(_collider, new ManualClock(1f / 30f), _config);
            var state = Grounded(Vector2.zero);
            state = motor.Step(state, Drive(Vector2.zero, 3f), out var launched);
            _collider.LaunchHeight = 0f;
            var apex = state.Position.y;

            for (var tick = 0; tick < 60 && state.Velocity.y > 0f; tick++)
            {
                state = motor.Step(state, BodyDrive.Free(state), out _);
                apex = Mathf.Max(apex, state.Position.y);
            }

            Assert.That(launched, Is.True);
            Assert.That(apex, Is.EqualTo(2f).Within(0.01f));
            Assert.That(apex, Is.LessThanOrEqualTo(2f + 1e-4f));
        }

        [Test]
        public void AirborneBodyIsNotLaunched()
        {
            _collider.LaunchHeight = 2f;

            var state = _motor.Step(Airborne(Vector2.zero), Drive(Vector2.zero), out var launched);

            Assert.That(launched, Is.False);
            Assert.That(state.Velocity.y, Is.LessThan(0f));
        }

        [Test]
        public void WalkerShovesWhatItReaches()
        {
            var crate = new RecordedShovable();
            _collider.Ahead.Add(new ShovableContact(crate, 0f));

            _motor.Step(Airborne(Vector2.zero), Drive(new Vector2(10f, 0f)), out _);

            Assert.That(crate.Shoves, Is.EqualTo(new[] { new Vector2(_config.ShoveForce, 0f) }));
        }

        [Test]
        public void ShoveIsSharedAmongEveryReachedBody()
        {
            var first = new RecordedShovable();
            var second = new RecordedShovable();
            var far = new RecordedShovable();
            _collider.Ahead.Add(new ShovableContact(first, 0f));
            _collider.Ahead.Add(new ShovableContact(second, 0.5f));
            _collider.Ahead.Add(new ShovableContact(far, 1.5f));

            _motor.Step(Airborne(Vector2.zero), Drive(new Vector2(10f, 0f)), out _);

            Assert.That(first.Shoves, Is.EqualTo(new[] { new Vector2(_config.ShoveForce / 2f, 0f) }));
            Assert.That(second.Shoves, Is.EqualTo(new[] { new Vector2(_config.ShoveForce / 2f, 0f) }));
            Assert.That(far.Shoves, Is.Empty);
        }

        [TestCase(0f, 1f)]
        [TestCase(5f, 0.5f)]
        [TestCase(7.5f, 0.25f)]
        public void ShoveWeakensAsTheBodyRetreats(float retreat, float effort)
        {
            var crate = new RecordedShovable { Retreat = retreat };
            _collider.Ahead.Add(new ShovableContact(crate, 0f));

            _motor.Step(Airborne(Vector2.zero), Drive(new Vector2(-10f, 0f)), out _);

            Assert.That(crate.Shoves, Is.EqualTo(new[] { new Vector2(-_config.ShoveForce * effort, 0f) }));
        }

        [TestCase(10f)]
        [TestCase(20f)]
        public void BodyRetreatingAsFastAsTheWalkerIsNotShoved(float retreat)
        {
            var crate = new RecordedShovable { Retreat = retreat };
            _collider.Ahead.Add(new ShovableContact(crate, 0f));

            _motor.Step(Airborne(Vector2.zero), Drive(new Vector2(-10f, 0f)), out _);

            Assert.That(crate.Shoves, Is.Empty);
        }

        [Test]
        public void PushedBodyThatDoesNotWalkShovesNothing()
        {
            var crate = new RecordedShovable();
            _collider.Ahead.Add(new ShovableContact(crate, 0f));

            _motor.Step(Pushed(5f, false), Drive(new Vector2(-1f, 0f)), out _);

            Assert.That(crate.Shoves, Is.Empty);
            Assert.That(_collider.Calls, Has.No.Member("ShovablesAhead right"));
        }

        private static BodyDrive Drive(Vector2 velocity, float gravityScale = 1f) =>
            new(velocity, gravityScale, false);

        private static BodyState Airborne(Vector2 position) =>
            new(position, Vector2.zero, 0f, false);

        private static BodyState Grounded(Vector2 position) =>
            new(position, Vector2.zero, 0f, true);

        private static BodyState Pushed(float push, bool grounded) =>
            new(Vector2.zero, Vector2.zero, push, grounded);
    }
}

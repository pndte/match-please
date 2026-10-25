using System;
using Bw.Tests.Doubles;
using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Physics;
using Bw.UseCases.Movement.Physics.Abstractions;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Bw.Tests.UseCases.Movement.Physics
{
    public sealed class PlatformerMotorTests
    {
        private const float Duration = 1f / 30f;

        private MovementConfig _config;
        private RecordedBodyMotor _body;
        private PlatformerMotor _motor;

        [SetUp]
        public void SetUp()
        {
            _config = ScriptableObject.CreateInstance<MovementConfig>();
            _config.Speed = 10f;
            _config.JumpForce = 20f;
            _config.AccelerationTime = 0.1f;
            _config.BrakingTime = 0.2f;
            _config.CoyoteTime = 0.1f;
            _config.JumpBufferTime = 0.1f;
            _config.ReleasedJumpGravity = 3f;
            _config.FallGravity = 1.5f;
            _body = new RecordedBodyMotor();
            _motor = new PlatformerMotor(_body, new ManualClock(Duration), _config);
        }

        [TearDown]
        public void TearDown() =>
            Object.DestroyImmediate(_config);

        [Test]
        public void RunFromRestGainsTheTopSpeedOverTheAccelerationTime()
        {
            _motor.Step(Grounded(0f), Run(1f));

            Assert.That(_body.Drive.Velocity.x, Is.EqualTo(10f / 0.1f * Duration).Within(1e-4f));
        }

        [Test]
        public void RunNeverExceedsTheTopSpeed()
        {
            _motor.Step(Grounded(9f), Run(1f));

            Assert.That(_body.Drive.Velocity.x, Is.EqualTo(10f));
        }

        [Test]
        public void ReleasedRunBrakesOverTheBrakingTime()
        {
            _motor.Step(Grounded(10f), Run(0f));

            Assert.That(_body.Drive.Velocity.x, Is.EqualTo(10f - 10f / 0.2f * Duration).Within(1e-4f));
        }

        [Test]
        public void TurningAroundBrakes()
        {
            _motor.Step(Grounded(10f), Run(-1f));

            Assert.That(_body.Drive.Velocity.x, Is.EqualTo(10f - 10f / 0.2f * Duration).Within(1e-4f));
        }

        [Test]
        public void SlowerInputThanTheCurrentRunBrakes()
        {
            _motor.Step(Grounded(10f), Run(0.5f));

            Assert.That(_body.Drive.Velocity.x, Is.EqualTo(10f - 10f / 0.2f * Duration).Within(1e-4f));
        }

        [Test]
        public void RunCountsTicksToTheTopSpeed()
        {
            var state = Grounded(0f);
            var ticks = 0;

            while (state.Body.Velocity.x < _config.Speed && ticks < 100)
            {
                state = _motor.Step(state, Run(1f));
                ticks++;
            }

            Assert.That(ticks, Is.EqualTo(3));
        }

        [Test]
        public void NoAccelerationTimeRunsAtOnce()
        {
            _config.AccelerationTime = 0f;

            _motor.Step(Grounded(0f), Run(1f));

            Assert.That(_body.Drive.Velocity.x, Is.EqualTo(10f));
        }

        [Test]
        public void NoBrakingTimeStopsAtOnce()
        {
            _config.BrakingTime = 0f;

            _motor.Step(Grounded(10f), Run(0f));

            Assert.That(_body.Drive.Velocity.x, Is.EqualTo(0f));
        }

        [TestCase(3f, 10f)]
        [TestCase(-7f, -10f)]
        [TestCase(0.5f, 5f)]
        public void InputIsClampedToTheTopSpeed(float horizontal, float speed)
        {
            _config.AccelerationTime = 0f;
            _config.BrakingTime = 0f;

            _motor.Step(Grounded(0f), Run(horizontal));

            Assert.That(_body.Drive.Velocity.x, Is.EqualTo(speed));
        }

        [Test]
        public void SlowdownLowersTheTopSpeed()
        {
            _config.AccelerationTime = 0f;

            _motor.Step(Grounded(0f), new MovementInput(1f, false, false, false, new Slowdown(0.5f, 0f)));

            Assert.That(_body.Drive.Velocity.x, Is.EqualTo(5f));
        }

        [Test]
        public void GroundedJumpLeavesWithTheJumpForceAndRises()
        {
            var state = _motor.Step(Grounded(0f), Jump(true));

            Assert.That(_body.Drive.Velocity.y, Is.EqualTo(20f));
            Assert.That(state.Jump.Rising, Is.True);
            Assert.That(state.Jump.CoyoteTicks, Is.EqualTo(0));
            Assert.That(state.Jump.BufferedTicks, Is.EqualTo(0));
        }

        [Test]
        public void SlowdownLowersTheJumpHeight()
        {
            _motor.Step(Grounded(0f), new MovementInput(0f, true, true, false, new Slowdown(0f, 0.75f)));

            Assert.That(_body.Drive.Velocity.y, Is.EqualTo(20f * Mathf.Sqrt(0.25f)).Within(1e-5f));
        }

        [Test]
        public void AirborneJumpWithoutCoyoteTimeIsBufferedInstead()
        {
            var state = _motor.Step(Airborne(-3f, coyoteTicks: 0, bufferedTicks: 0), Jump(true));

            Assert.That(_body.Drive.Velocity.y, Is.EqualTo(-3f));
            Assert.That(state.Jump.BufferedTicks, Is.EqualTo(3));
            Assert.That(state.Jump.Rising, Is.False);
        }

        [Test]
        public void CoyoteTimeAllowsAJumpJustAfterLeavingTheGround()
        {
            _motor.Step(Airborne(-3f, coyoteTicks: 1, bufferedTicks: 0), Jump(true));

            Assert.That(_body.Drive.Velocity.y, Is.EqualTo(20f));
        }

        [Test]
        public void BufferedPressJumpsOnLanding()
        {
            var state = _motor.Step(new MovementState(new BodyState(Vector2.zero, Vector2.zero, 0f, true), new JumpState(false, 0, 2)), Jump(false));

            Assert.That(_body.Drive.Velocity.y, Is.EqualTo(20f));
            Assert.That(state.Jump.BufferedTicks, Is.EqualTo(0));
        }

        [Test]
        public void GroundRefillsTheCoyoteTime()
        {
            var state = _motor.Step(Grounded(0f), Run(0f));

            Assert.That(state.Jump.CoyoteTicks, Is.EqualTo(3));
        }

        [TestCase(3, 2)]
        [TestCase(1, 0)]
        [TestCase(0, 0)]
        public void CoyoteTimeRunsOutInTheAir(int coyoteTicks, int left)
        {
            var state = _motor.Step(Airborne(-1f, coyoteTicks, 0), Run(0f));

            Assert.That(state.Jump.CoyoteTicks, Is.EqualTo(left));
        }

        [TestCase(3, 2)]
        [TestCase(1, 0)]
        [TestCase(0, 0)]
        public void BufferedPressRunsOut(int bufferedTicks, int left)
        {
            var state = _motor.Step(Airborne(-1f, 0, bufferedTicks), Run(0f));

            Assert.That(state.Jump.BufferedTicks, Is.EqualTo(left));
        }

        [Test]
        public void SpringboardLaunchEndsTheJumpAndTheCoyoteTime()
        {
            _body.Launching = true;

            var state = _motor.Step(Grounded(0f), Jump(true));

            Assert.That(state.Jump.Rising, Is.False);
            Assert.That(state.Jump.CoyoteTicks, Is.EqualTo(0));
        }

        [Test]
        public void RiseEndsWhenTheBodyStopsGoingUp()
        {
            _body.Outcome = drive => new BodyState(Vector2.zero, new Vector2(drive.Velocity.x, 0f), 0f, false);

            var state = _motor.Step(new MovementState(new BodyState(Vector2.zero, new Vector2(0f, 1f), 0f, false), new JumpState(true, 0, 0)), Jump(false));

            Assert.That(state.Jump.Rising, Is.False);
        }

        [Test]
        public void FallingBodyFallsUnderTheFallGravity()
        {
            _motor.Step(Airborne(-1f, 0, 0), Run(0f));

            Assert.That(_body.Drive.GravityScale, Is.EqualTo(1.5f));
        }

        [Test]
        public void HeldJumpRisesUnderPlainGravity()
        {
            _motor.Step(Rising(5f), new MovementInput(0f, false, true, false, default));

            Assert.That(_body.Drive.GravityScale, Is.EqualTo(1f));
        }

        [Test]
        public void ReleasedJumpRisesUnderTheReleasedJumpGravity()
        {
            _motor.Step(Rising(5f), Run(0f));

            Assert.That(_body.Drive.GravityScale, Is.EqualTo(3f));
        }

        [Test]
        public void RiseWithoutAJumpIgnoresTheReleasedButton()
        {
            _motor.Step(Airborne(5f, 0, 0), Run(0f));

            Assert.That(_body.Drive.GravityScale, Is.EqualTo(1f));
        }

        [Test]
        public void DownDropsThroughPlanks()
        {
            _motor.Step(Grounded(0f), new MovementInput(0f, false, false, true, default));

            Assert.That(_body.Drive.Dropping, Is.True);
        }

        [Test]
        public void WithoutDownTheBodyStaysOnPlanks()
        {
            _motor.Step(Grounded(0f), Run(1f));

            Assert.That(_body.Drive.Dropping, Is.False);
        }

        private static MovementInput Run(float horizontal) =>
            new(horizontal, false, false, false, default);

        private static MovementInput Jump(bool pressed) =>
            new(0f, pressed, true, false, default);

        private static MovementState Grounded(float run) =>
            new(new BodyState(Vector2.zero, new Vector2(run, 0f), 0f, true), new JumpState(false, 0, 0));

        private static MovementState Airborne(float fall, int coyoteTicks, int bufferedTicks) =>
            new(new BodyState(Vector2.zero, new Vector2(0f, fall), 0f, false), new JumpState(false, coyoteTicks, bufferedTicks));

        private static MovementState Rising(float rise) =>
            new(new BodyState(Vector2.zero, new Vector2(0f, rise), 0f, false), new JumpState(true, 0, 0));

        private sealed class RecordedBodyMotor : IBodyMotor
        {
            public BodyDrive Drive { get; private set; }
            public bool Launching { get; set; }
            public Func<BodyDrive, BodyState> Outcome { get; set; } = drive => new BodyState(Vector2.zero, drive.Velocity, 0f, false);

            public BodyState Step(BodyState state, BodyDrive drive, out bool launched)
            {
                Drive = drive;
                launched = Launching;
                var outcome = Outcome(drive);
                return new BodyState(outcome.Position, outcome.Velocity, outcome.HorizontalPush, state.Grounded);
            }
        }
    }
}

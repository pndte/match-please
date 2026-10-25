using System.Collections.Generic;
using Bw.Tests.Entities.Physics.Doubles;
using Bw.Tests.UseCases.Level.Doubles;
using Bw.UseCases.Level;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Level
{
    public sealed class MovingPlatformTests
    {
        private PhysicsTestWorld _world;
        private Lift _lift;

        [SetUp]
        public void SetUp()
        {
            _world = new PhysicsTestWorld();
            _lift = new Lift(_world, new Vector2(0f, 10f));
        }

        [TearDown]
        public void TearDown() =>
            _world.Dispose();

        [Test]
        public void PlatformStartsAtTheShareOfItsBodyOnTheRail()
        {
            Assert.That(_lift.Platform.State.Value.Along, Is.Zero.Within(1e-6f));
            Assert.That(_lift.Platform.Previous, Is.EqualTo(_lift.Platform.State.Value));
        }

        [Test]
        public void DrivenPlatformReachesTheShareAfterOnePhysicsStep()
        {
            _lift.Platform.DriveTo(0.1f);

            _world.Step();

            Assert.That(_lift.Platform.State.Value.Along, Is.EqualTo(0.1f).Within(1e-5f));
            Assert.That(_lift.Body.position, Is.EqualTo(Lift.Start + new Vector2(0f, 1f)).Using(Vector2Within(1e-4f)));
        }

        [Test]
        public void StepRemembersThePreviousStateAndAnnouncesTheNewOne()
        {
            var stepped = new List<PlatformState>();
            _lift.Platform.Stepped.Advise(_world.Lifetime, stepped.Add);
            _lift.Platform.DriveTo(0.1f);
            _world.Step();

            _lift.Platform.DriveTo(0.3f);
            _world.Step();

            Assert.That(_lift.Platform.Previous.Along, Is.EqualTo(0.1f).Within(1e-5f));
            Assert.That(stepped, Has.Count.EqualTo(2));
            Assert.That(stepped[1], Is.EqualTo(_lift.Platform.State.Value));
        }

        [Test]
        public void ShiftIsTheMoveOfTheLastStep()
        {
            _lift.Platform.DriveTo(0.1f);
            _world.Step();
            _lift.Platform.DriveTo(0.3f);

            _world.Step();

            Assert.That(_lift.Platform.Shift, Is.EqualTo(new Vector2(0f, 2f)).Using(Vector2Within(1e-4f)));
        }

        [Test]
        public void UndrivenPlatformStaysPut()
        {
            _world.Step(5);

            Assert.That(_lift.Platform.State.Value.Along, Is.Zero.Within(1e-6f));
            Assert.That(_lift.Platform.Shift, Is.EqualTo(Vector2.zero));
        }

        [Test]
        public void ApplyPlacesTheBodyAtTheShareWithoutAStep()
        {
            var stepped = 0;
            _lift.Platform.Stepped.Advise(_world.Lifetime, _ => stepped++);

            _lift.Platform.Apply(new PlatformState(0.5f));

            Assert.That(_lift.Platform.State.Value.Along, Is.EqualTo(0.5f));
            Assert.That(_lift.Body.position, Is.EqualTo(Lift.Start + new Vector2(0f, 5f)).Using(Vector2Within(1e-5f)));
            Assert.That(stepped, Is.Zero);
        }

        [Test]
        public void AppliedStateIsWhereTheNextStepStarts()
        {
            _lift.Platform.Apply(new PlatformState(0.5f));
            _lift.Platform.DriveTo(0.6f);

            _world.Step();

            Assert.That(_lift.Platform.Previous.Along, Is.EqualTo(0.5f));
            Assert.That(_lift.Platform.State.Value.Along, Is.EqualTo(0.6f).Within(1e-5f));
        }

        [Test]
        public void BoxIsAtTheCurrentShare()
        {
            _lift.Platform.Apply(new PlatformState(0.5f));

            var area = _lift.Platform.BoxAt(0).Area;

            Assert.That(area.center, Is.EqualTo(Lift.Start + new Vector2(0f, 5f)).Using(Vector2Within(1e-5f)));
            Assert.That(area.size, Is.EqualTo(Lift.Size).Using(Vector2Within(1e-5f)));
        }

        private static IEqualityComparer<Vector2> Vector2Within(float tolerance) =>
            new Vector2Tolerance(tolerance);

        private sealed class Vector2Tolerance : IEqualityComparer<Vector2>
        {
            private readonly float _tolerance;

            public Vector2Tolerance(float tolerance)
            {
                _tolerance = tolerance;
            }

            public bool Equals(Vector2 x, Vector2 y) =>
                (x - y).magnitude <= _tolerance;

            public int GetHashCode(Vector2 value) =>
                0;
        }
    }
}

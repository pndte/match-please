using System.Collections.Generic;
using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Shooting;
using Bw.UseCases.Combat.Shooting.Abstractions;
using Bw.UseCases.Combat.Shooting.Network;
using Bw.UseCases.Combat.Weapon.Abstractions;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Shooting.Network
{
    public sealed class SpreadKickerTests
    {
        private ManualControl _control;
        private RecordingSpread _spread;
        private RecordingDeflection _deflection;
        private SpreadKicker _kicker;

        [SetUp]
        public void SetUp()
        {
            _control = new ManualControl(true);
            _spread = new RecordingSpread();
            _deflection = new RecordingDeflection();
            _kicker = new SpreadKicker(_control, _spread, _deflection);
        }

        [Test]
        public void FirstShownStateOnlySynchronizesTheShotCount()
        {
            _kicker.Show(Shots(0), Shots(5), 0.5f);

            Assert.That(_deflection.Kicks, Is.Empty);
        }

        [Test]
        public void NewShotOfAControlledGunKicksTheBarrelByTheSpreadsThrow()
        {
            _kicker.Show(Shots(0), Shots(5), 0f);

            _kicker.Show(Shots(5), Shots(6, 30f), 0f);

            Assert.That(_deflection.Kicks, Is.EqualTo(new[] { RecordingSpread.Thrown }));
            Assert.That(_spread.Aims, Is.EqualTo(new[] { 30f }));
        }

        [Test]
        public void RollOfTheJitterStaysWithinOneEitherWay()
        {
            _kicker.Show(Shots(0), Shots(0), 0f);

            for (var shot = 1; shot <= 50; shot++)
                _kicker.Show(Shots(shot - 1), Shots(shot), 0f);

            Assert.That(_spread.Rolls, Has.Count.EqualTo(50));
            Assert.That(_spread.Rolls, Has.All.InRange(-1f, 1f));
        }

        [Test]
        public void SameShotShownAgainKicksOnce()
        {
            _kicker.Show(Shots(0), Shots(0), 0f);
            _kicker.Show(Shots(0), Shots(1), 0.2f);

            _kicker.Show(Shots(0), Shots(1), 0.6f);

            Assert.That(_deflection.Kicks, Has.Count.EqualTo(1));
        }

        [Test]
        public void SeveralShotsBetweenTwoFramesKickOnce()
        {
            _kicker.Show(Shots(0), Shots(0), 0f);

            _kicker.Show(Shots(0), Shots(3), 0f);

            Assert.That(_deflection.Kicks, Has.Count.EqualTo(1));
        }

        [Test]
        public void GunNotControlledHereNeverKicks()
        {
            _control.Release();
            _kicker.Show(Shots(0), Shots(0), 0f);

            _kicker.Show(Shots(0), Shots(1), 0f);

            Assert.That(_deflection.Kicks, Is.Empty);
        }

        [Test]
        public void ShotsFiredWhileNotControlledAreNotKickedLater()
        {
            _kicker.Show(Shots(0), Shots(0), 0f);
            _control.Release();
            _kicker.Show(Shots(0), Shots(4), 0f);
            _control.Take();

            _kicker.Show(Shots(4), Shots(4), 0f);

            Assert.That(_deflection.Kicks, Is.Empty);
        }

        [Test]
        public void ShotCountGoingBackKicksNothing()
        {
            _kicker.Show(Shots(0), Shots(5), 0f);

            _kicker.Show(Shots(5), Shots(4), 0f);

            Assert.That(_deflection.Kicks, Is.Empty);
        }

        private static GunState Shots(int shots, float aim = 0f) =>
            new(aim, 3, 0, 0, shots);

        private sealed class RecordingSpread : ISpread
        {
            public const float Thrown = 4.5f;

            public List<float> Aims { get; } = new();
            public List<float> Rolls { get; } = new();

            public float ThrowOf(float aim, float roll)
            {
                Aims.Add(aim);
                Rolls.Add(roll);
                return Thrown;
            }

            public float Returned(float deflection, float seconds) =>
                deflection;
        }

        private sealed class RecordingDeflection : IAimDeflection
        {
            public float Current => 0f;
            public List<float> Kicks { get; } = new();

            public void Kick(float degrees) =>
                Kicks.Add(degrees);
        }
    }
}

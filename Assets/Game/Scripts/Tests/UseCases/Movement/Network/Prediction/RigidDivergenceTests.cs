using Bw.UseCases.Movement;
using Bw.UseCases.Movement.Network.Prediction;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement.Network.Prediction
{
    public sealed class RigidDivergenceTests
    {
        private static readonly RigidState Crate = new(new Vector2(1f, 2f), 30f, new Vector2(3f, 0f), 10f);
        private static readonly RigidDivergence Divergence = new();

        [Test]
        public void SameStateDoesNotDiffer()
        {
            Assert.That(Divergence.Diverged(Crate, Crate), Is.False);
        }

        [Test]
        public void SpinIsNotCompared()
        {
            var spun = new RigidState(Crate.Position, Crate.Angle, Crate.Velocity, -500f);

            Assert.That(Divergence.Diverged(Crate, spun), Is.False);
        }

        [TestCase(0.009f, false)]
        [TestCase(0.011f, true)]
        public void PositionDiffersPastOneCentimetre(float shift, bool differs)
        {
            var moved = new RigidState(Crate.Position + new Vector2(0f, shift), Crate.Angle, Crate.Velocity, Crate.AngularVelocity);

            Assert.That(Divergence.Diverged(Crate, moved), Is.EqualTo(differs));
        }

        [TestCase(0.9f, false)]
        [TestCase(1.1f, true)]
        [TestCase(-1.1f, true)]
        public void AngleDiffersPastOneDegree(float turn, bool differs)
        {
            var turned = new RigidState(Crate.Position, Crate.Angle + turn, Crate.Velocity, Crate.AngularVelocity);

            Assert.That(Divergence.Diverged(Crate, turned), Is.EqualTo(differs));
        }

        [Test]
        public void AngleIsComparedAcrossTheFullTurn()
        {
            var nearlyFull = new RigidState(Crate.Position, 359.6f, Crate.Velocity, Crate.AngularVelocity);
            var justPast = new RigidState(Crate.Position, 0.3f, Crate.Velocity, Crate.AngularVelocity);

            Assert.That(Divergence.Diverged(nearlyFull, justPast), Is.False);
        }

        [TestCase(0.09f, false)]
        [TestCase(0.11f, true)]
        public void VelocityDiffersPastATenthOfAMetrePerSecond(float change, bool differs)
        {
            var faster = new RigidState(Crate.Position, Crate.Angle, Crate.Velocity + new Vector2(change, 0f), Crate.AngularVelocity);

            Assert.That(Divergence.Diverged(Crate, faster), Is.EqualTo(differs));
        }
    }
}

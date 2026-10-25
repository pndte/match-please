using Bw.UseCases.Movement;
using NUnit.Framework;
using UnityEngine;

namespace Bw.Tests.UseCases.Movement
{
    public sealed class BodyDriveTests
    {
        [Test]
        public void FreeBodyKeepsItsFallUnderPlainGravityWithoutAWalk()
        {
            var drive = BodyDrive.Free(new BodyState(Vector2.zero, new Vector2(7f, -3f), 2f, false));

            Assert.That(drive.Velocity, Is.EqualTo(new Vector2(0f, -3f)));
            Assert.That(drive.GravityScale, Is.EqualTo(1f));
            Assert.That(drive.Dropping, Is.False);
        }
    }
}

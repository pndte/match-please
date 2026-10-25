using Bw.UseCases.Combat.Shooting;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Shooting
{
    public sealed class FlashlightTests
    {
        [Test]
        public void FlashlightStartsOff()
        {
            Assert.That(new Flashlight().Lit.Value, Is.False);
        }

        [Test]
        public void AppliedStateIsTheLitState()
        {
            var flashlight = new Flashlight();

            flashlight.Apply(true);

            Assert.That(flashlight.Lit.Value, Is.True);
        }

        [Test]
        public void FlashlightTurnsOffAgain()
        {
            var flashlight = new Flashlight();
            flashlight.Apply(true);

            flashlight.Apply(false);

            Assert.That(flashlight.Lit.Value, Is.False);
        }
    }
}

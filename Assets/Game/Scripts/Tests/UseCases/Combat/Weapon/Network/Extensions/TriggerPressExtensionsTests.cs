using Bw.UseCases.Combat.Weapon.Network.Extensions;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network.Extensions
{
    public sealed class TriggerPressExtensionsTests
    {
        private static readonly TriggerPress Press = new(9, 4, 2.5f, 1.25f, 6);

        [TestCase(0, 0)]
        [TestCase(7, 7)]
        [TestCase(255, 255)]
        [TestCase(300, 255)]
        [TestCase(-1, 0)]
        public void AgeIsKeptInAByte(int age, int kept)
        {
            Assert.That(Press.WithAge(age).Age, Is.EqualTo(kept));
        }

        [Test]
        public void AgedPressKeepsEverythingElse()
        {
            Assert.That(Press.WithAge(1), Is.EqualTo(new TriggerPress(9, 1, 2.5f, 1.25f, 6)));
        }

        [Test]
        public void DeliveredPressIsFreshWithItsLateness()
        {
            Assert.That(Press.Delivered(3), Is.EqualTo(new TriggerPress(9, 0, 2.5f, 1.25f, 3)));
        }

        [TestCase(1000, 255)]
        [TestCase(-5, 0)]
        public void LatenessIsKeptInAByte(int late, int kept)
        {
            Assert.That(Press.Delivered(late).Late, Is.EqualTo(kept));
        }
    }
}

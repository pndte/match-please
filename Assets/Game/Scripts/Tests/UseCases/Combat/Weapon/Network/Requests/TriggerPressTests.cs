using Bw.UseCases.Combat.Weapon.Network.Requests;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network.Requests
{
    public sealed class TriggerPressTests
    {
        private static readonly TriggerPress Sample = new(9, 4, 2.5f, 1.25f, 6);

        private static readonly TriggerPress[] Others =
        {
            new(8, 4, 2.5f, 1.25f, 6),
            new(9, 5, 2.5f, 1.25f, 6),
            new(9, 4, 2.6f, 1.25f, 6),
            new(9, 4, 2.5f, 1.5f, 6),
            new(9, 4, 2.5f, 1.25f, 7)
        };

        [Test]
        public void PressesWithTheSameFieldsAreEqual()
        {
            Assert.That(Sample.Equals(new TriggerPress(9, 4, 2.5f, 1.25f, 6)), Is.True);
        }

        [TestCaseSource(nameof(Others))]
        public void PressesDifferingInAnyFieldDiffer(TriggerPress other)
        {
            Assert.That(Sample.Equals(other), Is.False);
        }
    }
}

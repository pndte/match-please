using Bw.UseCases.Combat.Weapon.Network;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network
{
    public sealed class CharacterTriggerTests
    {
        [Test]
        public void TriggerStartsReleased()
        {
            Assert.That(new CharacterTrigger().Current.Pulled, Is.False);
        }

        [Test]
        public void TriggerIsTheLastControlsTaken()
        {
            var trigger = new CharacterTrigger();
            var press = new TriggerPress(3, 0, 1f, 2f, 0);
            trigger.Take(new TriggerControls(true, 4f, 5f, press));

            trigger.Take(new TriggerControls(false, 6f, 7f, press));

            Assert.That(trigger.Current.Pulled, Is.False);
            Assert.That(trigger.Current.ViewDelay, Is.EqualTo(6f));
            Assert.That(trigger.Current.PredictedDelay, Is.EqualTo(7f));
            Assert.That(trigger.Current.Press, Is.EqualTo(press));
        }
    }
}

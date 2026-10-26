using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Weapon.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network
{
    public sealed class CharacterTriggerRegistrarTests
    {
        [Test]
        public void CharactersTriggerIsRegisteredForItsLifetime()
        {
            var registration = new LifetimeDefinition();
            var character = new FakeCharacter();
            var trigger = new CharacterTrigger();
            var triggers = new CharacterTriggers();

            new CharacterTriggerRegistrar(registration.Lifetime, character, trigger, triggers);

            Assert.That(triggers.ByCharacter[character], Is.SameAs(trigger));

            registration.Terminate();

            Assert.That(triggers.ByCharacter.ContainsKey(character), Is.False);
        }
    }
}

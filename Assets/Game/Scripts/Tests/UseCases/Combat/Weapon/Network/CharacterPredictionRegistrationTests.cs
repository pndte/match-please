using Bw.Tests.UseCases.Combat.Weapon.Doubles;
using Bw.UseCases.Combat.Weapon.Network;
using JetBrains.Lifetimes;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Combat.Weapon.Network
{
    public sealed class CharacterPredictionRegistrationTests
    {
        [Test]
        public void CharactersPredictionIsRegisteredForItsLifetime()
        {
            var registration = new LifetimeDefinition();
            var character = new FakeCharacter();
            var predictedBy = new ManualControl();
            var predictions = new CharacterPredictions();

            new CharacterPredictionRegistration(registration.Lifetime, character, predictedBy, predictions);

            Assert.That(predictions.ByCharacter[character], Is.SameAs(predictedBy));

            registration.Terminate();

            Assert.That(predictions.ByCharacter.ContainsKey(character), Is.False);
        }
    }
}

using Bw.Entities.Players;
using Bw.Tests.UseCases.PropsMatchBots.Doubles;
using Bw.UseCases;
using Bw.UseCases.Match.Network;
using NUnit.Framework;

namespace Bw.Tests.UseCases.Match.Network
{
    public sealed class CombatantsTests
    {
        private CharacterRegistry _registry;
        private Combatants _combatants;

        [SetUp]
        public void SetUp()
        {
            _registry = new CharacterRegistry();
            _combatants = new Combatants(_registry);
        }

        [Test]
        public void PlayerOfARegisteredCharacterIsFound()
        {
            var character = new StubCharacter();
            var player = new Player("Сева");
            _registry.PlayerByCharacter.Add(character, player);

            Assert.That(_combatants.TryFindPlayer(character, out var found), Is.True);
            Assert.That(found, Is.SameAs(player));
        }

        [Test]
        public void UnregisteredCharacterHasNoPlayer()
        {
            Assert.That(_combatants.TryFindPlayer(new StubCharacter(), out _), Is.False);
        }

        [Test]
        public void VictimIsThePlayerWhoseCharacterIsTheMortal()
        {
            var first = new StubCharacter();
            var second = new StubCharacter();
            var player = new Player("Гошак");
            _registry.PlayerByCharacter.Add(first, new Player("Сева"));
            _registry.PlayerByCharacter.Add(second, player);

            Assert.That(_combatants.TryFindVictim(second.Mortal, out var victim), Is.True);
            Assert.That(victim, Is.SameAs(player));
        }

        [Test]
        public void MortalOfNoCharacterIsNoVictim()
        {
            _registry.PlayerByCharacter.Add(new StubCharacter(), new Player("Сева"));

            Assert.That(_combatants.TryFindVictim(new StubMortal(), out var victim), Is.False);
            Assert.That(victim, Is.Null);
        }
    }
}

using System;
using Bw.Entities.Network;
using Bw.Entities.Players;
using NUnit.Framework;

namespace Bw.Tests.Entities.Network
{
    public sealed class DepartedPlayersTests
    {
        private static readonly Guid Token = Guid.NewGuid();

        [Test]
        public void DepartedPlayerIsFoundByItsToken()
        {
            var departed = new DepartedPlayers();
            var player = new Player("Саня");

            departed.Add(player, Token);

            Assert.That(departed.Contains(Token), Is.True);
            Assert.That(departed.PlayerOf(Token), Is.SameAs(player));
        }

        [Test]
        public void RemovedPlayerIsNoLongerDeparted()
        {
            var departed = new DepartedPlayers();
            var player = new Player("Саня");
            departed.Add(player, Token);

            departed.Remove(player);

            Assert.That(departed.Contains(Token), Is.False);
            Assert.That(departed.ByPlayer.Count, Is.EqualTo(0));
        }

        [Test]
        public void UnknownTokenHasNoPlayer()
        {
            var departed = new DepartedPlayers();

            Assert.That(departed.Contains(Token), Is.False);
            Assert.Throws<InvalidOperationException>(() => departed.PlayerOf(Token));
        }

        [Test]
        public void PlayerThatHasNotDepartedCannotReturn()
        {
            Assert.Throws<InvalidOperationException>(() => new DepartedPlayers().Remove(new Player("Саня")));
        }
    }
}

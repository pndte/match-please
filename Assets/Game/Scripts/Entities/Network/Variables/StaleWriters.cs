using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.Entities.Players;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Variables
{
    internal sealed class StaleWriters
    {
        private readonly List<IPlayer> _players = new();
        private readonly float _seconds;

        public StaleWriters(Lifetime lifetime, IOwnerRegistry ownerRegistry, float seconds)
        {
            _seconds = seconds;
            ownerRegistry.Owners.View(lifetime, (ownerLifetime, owner) =>
                ownerLifetime.OnTermination(() => KeepFor(lifetime, owner)));
        }

        public bool Listed(IPlayer player) => _players.Contains(player);

        private void KeepFor(Lifetime lifetime, IPlayer formerOwner) =>
            lifetime.CreateNestedFor(_seconds).TryBracket(() => _players.Add(formerOwner), () => _players.Remove(formerOwner));
    }
}

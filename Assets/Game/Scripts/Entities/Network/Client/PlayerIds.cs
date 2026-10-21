using System;
using System.Collections.Generic;
using Bw.Entities.Players;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network
{
    public sealed class PlayerIds : IPlayerIds
    {
        private readonly Dictionary<IPlayer, int> _ids = new();

        private int _issued;

        public int IdOf(IPlayer player)
        {
            if (!_ids.TryGetValue(player, out var id))
                throw new InvalidOperationException($"Player '{player.Nickname}' has no network id: whoever brings a player into the session gives it one.");

            return id;
        }

        public void AddLifetimed(Lifetime lifetime, IPlayer player)
        {
            if (!_ids.TryAdd(player, _issued + 1))
                throw new InvalidOperationException($"Player '{player.Nickname}' has one network id: it was added again before its last id was taken back.");

            _issued++;
            lifetime.OnTermination(() => _ids.Remove(player));
        }
    }
}

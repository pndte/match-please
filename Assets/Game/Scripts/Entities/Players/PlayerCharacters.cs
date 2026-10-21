using System;
using System.Collections.Generic;
using JetBrains.Lifetimes;

namespace Bw.Entities.Players
{
    public sealed class PlayerCharacters : IPlayerCharacters
    {
        private readonly Dictionary<IPlayer, int> _keys = new();

        public int KeyOf(IPlayer player)
        {
            if (!_keys.TryGetValue(player, out var key))
                throw new InvalidOperationException($"Player '{player.Nickname}' has no character: whoever brings a player into the match names the character it plays.");

            return key;
        }

        public void AddLifetimed(Lifetime lifetime, IPlayer player, int characterKey)
        {
            if (!_keys.TryAdd(player, characterKey))
                throw new InvalidOperationException($"Player '{player.Nickname}' plays one character at a time: it was named again before its last naming ended.");

            lifetime.OnTermination(() => _keys.Remove(player));
        }
    }
}

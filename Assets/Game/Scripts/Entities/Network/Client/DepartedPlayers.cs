using System;
using Bw.Entities.Players;

namespace Bw.Entities.Network
{
    public interface IReadonlyDepartedPlayers
    {
        public IReadonlyViewableMap<IPlayer, Guid> ByPlayer { get; }
    }

    public sealed class DepartedPlayers : IReadonlyDepartedPlayers
    {
        public IReadonlyViewableMap<IPlayer, Guid> ByPlayer => _tokens;

        private readonly BwViewableMap<IPlayer, Guid> _tokens = new();

        public bool Contains(Guid token)
        {
            foreach (var departed in _tokens)
                if (departed.Value == token)
                    return true;

            return false;
        }

        public IPlayer PlayerOf(Guid token)
        {
            foreach (var departed in _tokens)
                if (departed.Value == token)
                    return departed.Key;

            throw new InvalidOperationException("No departed player left with this token: a client returns only to a player its token left behind.");
        }

        public void Add(IPlayer player, Guid token) =>
            _tokens.Add(player, token);

        public void Remove(IPlayer player)
        {
            if (!_tokens.Remove(player))
                throw new InvalidOperationException($"Player '{player.Nickname}' has not departed: only a departed player returns.");
        }
    }
}

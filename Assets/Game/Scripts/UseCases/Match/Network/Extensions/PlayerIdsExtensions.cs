using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.UseCases.Match.Network.Requests;

namespace Bw.UseCases.Match.Network.Extensions
{
    public static class PlayerIdsExtensions
    {
        public static PlayerTag TagOf(this IReadonlyPlayerIds ids, IPlayer player) =>
            new(ids.IdOf(player), player.Nickname);
    }
}

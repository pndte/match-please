using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.UseCases.Bots.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Bots.Network.Room
{
    public sealed class RoomBotsSeeder
    {
        public RoomBotsSeeder(
            Lifetime lifetime,
            INetworkHolder networkHolder,
            RoomSettings room,
            BotPresetCatalog presets,
            IBotRoster roster)
        {
            networkHolder.NetworkManager.AdviseNotNull(lifetime, _ =>
            {
                foreach (var bot in room.Bots)
                    roster.Add(NicknameRules.Accepted(bot.Nickname), presets.PresetOf(bot.DifficultyKey, bot.PersonalityKey));
            });
        }
    }
}

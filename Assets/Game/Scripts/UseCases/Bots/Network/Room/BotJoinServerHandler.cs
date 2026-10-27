using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Entities.Players;
using Bw.UseCases.Bots.Extensions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Bots.Network.Room
{
    public sealed class BotJoinServerHandler
    {
        private readonly IBotRoster _roster;
        private readonly BotPresetCatalog _presets;

        public BotJoinServerHandler(
            Lifetime lifetime,
            INetRequestReceiver<RoomBot> joins,
            IBotRoster roster,
            BotPresetCatalog presets)
        {
            _roster = roster;
            _presets = presets;

            joins.Received.Advise(lifetime, Join);
        }

        private void Join(RoomBot bot) =>
            _roster.Add(NicknameRules.Accepted(bot.Nickname), _presets.PresetOf(bot.DifficultyKey, bot.PersonalityKey));
    }
}

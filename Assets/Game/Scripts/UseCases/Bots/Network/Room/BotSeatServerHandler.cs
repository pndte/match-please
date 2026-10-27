using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.Entities.Players;
using Bw.UseCases.Bots.Extensions;
using Bw.UseCases.Bots.Network.Requests;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Bots.Network.Room
{
    public sealed class BotSeatServerHandler
    {
        private readonly IBotRoster _roster;
        private readonly IReadonlyPlayerIds _ids;
        private readonly BotPresetCatalog _presets;

        public BotSeatServerHandler(
            Lifetime lifetime,
            INetRequestReceiver<int> removals,
            INetRequestReceiver<BotPresetChange> changes,
            IBotRoster roster,
            IReadonlyPlayerIds ids,
            BotPresetCatalog presets)
        {
            _roster = roster;
            _ids = ids;
            _presets = presets;

            removals.Received.Advise(lifetime, Remove);
            changes.Received.Advise(lifetime, Change);
        }

        private void Remove(int playerId) =>
            _roster.Remove(BotOf(playerId));

        private void Change(BotPresetChange change) =>
            _roster.Change(BotOf(change.PlayerId), _presets.PresetOf(change.DifficultyKey, change.PersonalityKey));

        private IPlayer BotOf(int playerId)
        {
            foreach (var bot in _roster.Bots.Keys)
                if (_ids.IdOf(bot) == playerId)
                    return bot;

            throw new InvalidOperationException($"Player {playerId.ToString()} is no bot of this room: the host asked to change a bot that left.");
        }
    }
}

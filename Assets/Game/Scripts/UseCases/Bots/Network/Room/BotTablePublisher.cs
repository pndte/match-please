using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Bots.Network.Requests;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Bots.Network.Room
{
    public sealed class BotTablePublisher
    {
        private readonly IReadonlyBotRoster _roster;
        private readonly IReadonlyPlayerIds _ids;
        private readonly IViewableProperty<BotTable> _table;

        public BotTablePublisher(
            Lifetime lifetime,
            IReadonlyBotRoster roster,
            IReadonlyPlayerIds ids,
            INetEntries entries,
            NetPropertyDeclaration<BotTable> declaration)
        {
            _roster = roster;
            _ids = ids;
            _table = entries.Property(declaration);

            roster.Bots.Advise(lifetime, _ => Publish());
        }

        private void Publish()
        {
            var entries = new List<BotTableEntry>(_roster.Bots.Count);
            foreach (var (bot, entry) in _roster.Bots)
                entries.Add(new BotTableEntry(_ids.IdOf(bot), entry.Nickname, entry.Difficulty.Key, entry.Personality.Key));

            _table.Value = new BotTable(entries);
        }
    }
}

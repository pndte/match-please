using System.Collections.Generic;
using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using Bw.UseCases.Bots.Extensions;
using Bw.UseCases.Bots.Network.Requests;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Bots.Network.Room
{
    public sealed class BotTableReceiver
    {
        private readonly BotPresetCatalog _presets;
        private readonly IBotSeats _seats;

        public BotTableReceiver(
            Lifetime lifetime,
            INetEntries entries,
            NetPropertyDeclaration<BotTable> declaration,
            BotPresetCatalog presets,
            IBotSeats seats)
        {
            _presets = presets;
            _seats = seats;

            entries.Property(declaration).Advise(lifetime, Apply);
        }

        private void Apply(BotTable table)
        {
            var seats = new List<BotSeat>(table.Entries.Count);
            foreach (var entry in table.Entries)
                seats.Add(new BotSeat(entry.PlayerId, entry.Nickname, _presets.PresetOf(entry.DifficultyKey, entry.PersonalityKey)));

            _seats.Apply(seats);
        }
    }
}

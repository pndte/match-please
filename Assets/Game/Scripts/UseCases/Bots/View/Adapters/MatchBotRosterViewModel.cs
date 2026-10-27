using System;
using System.Collections.Generic;
using Bw.CoreAdapters.Bots;
using Bw.UseCases.Bots.Extensions;
using Bw.UseCases.Bots.Network.Room;
using Bw.UseCases.Bots.View.Roster;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Bots.View.Adapters
{
    public sealed class MatchBotRosterViewModel : IBotRosterViewModel
    {
        public IReadonlyProperty<IReadOnlyList<BotRow>> Rows => _rows;
        public IReadonlyProperty<bool> Full => _full;

        private readonly ViewableProperty<IReadOnlyList<BotRow>> _rows = new(Array.Empty<BotRow>());
        private readonly ViewableProperty<bool> _full = new(false);
        private readonly Random _random = new();
        private readonly IReadonlyBotSeats _seats;
        private readonly IBotOrders _orders;
        private readonly BotPresetCatalog _presets;
        private readonly BotPresetLooksConfig _looks;
        private readonly RoomBotsConfig _room;

        public MatchBotRosterViewModel(
            Lifetime lifetime,
            IReadonlyBotSeats seats,
            IBotOrders orders,
            BotPresetCatalog presets,
            BotPresetLooksConfig looks,
            RoomBotsConfig room)
        {
            _seats = seats;
            _orders = orders;
            _presets = presets;
            _looks = looks;
            _room = room;

            seats.Seats.Advise(lifetime, Show);
        }

        public void Add() =>
            _orders.Add(BotNames.Pick(_room.Names, TakenNicknames(), _random), _presets.DefaultPreset(_random));

        public void Remove(int row) =>
            _orders.Remove(_seats.Seats.Value[row]);

        public void CycleDifficulty(int row)
        {
            var seat = _seats.Seats.Value[row];
            _orders.Change(seat, new BotPreset(_presets.DifficultyAfter(seat.Preset.Difficulty), seat.Preset.Personality));
        }

        public void CyclePersonality(int row)
        {
            var seat = _seats.Seats.Value[row];
            _orders.Change(seat, new BotPreset(seat.Preset.Difficulty, _presets.PersonalityAfter(seat.Preset.Personality)));
        }

        private void Show(IReadOnlyList<BotSeat> seats)
        {
            var rows = new BotRow[seats.Count];
            for (var index = 0; index < seats.Count; index++)
                rows[index] = new BotRow(seats[index].Nickname, _looks.NameOf(seats[index].Preset.Difficulty), _looks.NameOf(seats[index].Preset.Personality));

            _rows.Value = rows;
            _full.Value = seats.Count >= _room.MaxBots;
        }

        private HashSet<string> TakenNicknames()
        {
            var taken = new HashSet<string>();
            foreach (var seat in _seats.Seats.Value)
                taken.Add(seat.Nickname);

            return taken;
        }
    }
}

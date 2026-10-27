using System;
using System.Collections.Generic;
using Bw.CoreAdapters.Bots;
using Bw.Entities.Network;
using Bw.UseCases.Bots;
using Bw.UseCases.Bots.Extensions;
using Bw.UseCases.Bots.View.Roster;
using JetBrains.Collections.Viewable;

namespace Setup.Launch.Menu
{
    public sealed class RoomBotsViewModel : IBotRosterViewModel
    {
        public IReadonlyProperty<IReadOnlyList<BotRow>> Rows => _rows;
        public IReadonlyProperty<bool> Full => _full;

        public IReadOnlyList<RoomBot> RoomBots
        {
            get
            {
                var bots = new RoomBot[_seats.Count];
                for (var index = 0; index < _seats.Count; index++)
                    bots[index] = new RoomBot(_seats[index].Nickname, _seats[index].Preset.Difficulty.Key, _seats[index].Preset.Personality.Key);

                return bots;
            }
        }

        private readonly ViewableProperty<IReadOnlyList<BotRow>> _rows = new(Array.Empty<BotRow>());
        private readonly ViewableProperty<bool> _full = new(false);
        private readonly List<Seat> _seats = new();
        private readonly Random _random = new();
        private readonly BotPresetCatalog _presets;
        private readonly BotPresetLooksConfig _looks;
        private readonly RoomBotsConfig _room;

        public RoomBotsViewModel(BotPresetCatalog presets, BotPresetLooksConfig looks, RoomBotsConfig room)
        {
            _presets = presets;
            _looks = looks;
            _room = room;
        }

        public void Add()
        {
            if (_seats.Count >= _room.MaxBots)
                throw new InvalidOperationException($"A room holds {_room.MaxBots.ToString()} bots at most: the add button is locked when it is full.");

            _seats.Add(new Seat(BotNames.Pick(_room.Names, TakenNicknames(), _random), _presets.DefaultPreset(_random)));
            Show();
        }

        public void Remove(int row)
        {
            _seats.RemoveAt(row);
            Show();
        }

        public void CycleDifficulty(int row)
        {
            var seat = _seats[row];
            _seats[row] = new Seat(seat.Nickname, new BotPreset(_presets.DifficultyAfter(seat.Preset.Difficulty), seat.Preset.Personality));
            Show();
        }

        public void CyclePersonality(int row)
        {
            var seat = _seats[row];
            _seats[row] = new Seat(seat.Nickname, new BotPreset(seat.Preset.Difficulty, _presets.PersonalityAfter(seat.Preset.Personality)));
            Show();
        }

        private void Show()
        {
            var rows = new BotRow[_seats.Count];
            for (var index = 0; index < _seats.Count; index++)
                rows[index] = new BotRow(_seats[index].Nickname, _looks.NameOf(_seats[index].Preset.Difficulty), _looks.NameOf(_seats[index].Preset.Personality));

            _rows.Value = rows;
            _full.Value = _seats.Count >= _room.MaxBots;
        }

        private HashSet<string> TakenNicknames()
        {
            var taken = new HashSet<string>();
            foreach (var seat in _seats)
                taken.Add(seat.Nickname);

            return taken;
        }

        private readonly struct Seat
        {
            public readonly string Nickname;
            public readonly BotPreset Preset;

            public Seat(string nickname, BotPreset preset)
            {
                Nickname = nickname;
                Preset = preset;
            }
        }
    }
}

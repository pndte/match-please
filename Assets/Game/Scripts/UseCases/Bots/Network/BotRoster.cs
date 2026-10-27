using System;
using System.Collections.Generic;
using Bw.CoreAdapters.Characters;
using Bw.Entities;
using Bw.Entities.Extensions;
using Bw.Entities.Network;
using Bw.Entities.Players;
using Bw.UseCases.Character;
using JetBrains.Lifetimes;
using Random = UnityEngine.Random;

namespace Bw.UseCases.Bots.Network
{
    public sealed class BotRoster : IBotRoster
    {
        public IReadonlyViewableMap<IPlayer, BotEntry> Bots => _bots;

        private readonly BwViewableMap<IPlayer, BotEntry> _bots = new();
        private readonly Dictionary<IPlayer, LifetimeDefinition> _seats = new();
        private readonly Lifetime _lifetime;
        private readonly IPlayerAdmitter _admitter;
        private readonly CharacterCatalog _characters;
        private readonly RoomBotsConfig _room;

        public BotRoster(
            Lifetime lifetime,
            IPlayerAdmitter admitter,
            CharacterCatalog characters,
            RoomBotsConfig room)
        {
            _lifetime = lifetime;
            _admitter = admitter;
            _characters = characters;
            _room = room;
        }

        public void Add(string nickname, BotPreset preset)
        {
            if (_bots.Count >= _room.MaxBots)
                throw new InvalidOperationException($"The room holds {_room.MaxBots.ToString()} bots at most: remove one before adding '{nickname}'.");

            if (IsTaken(nickname))
                throw new InvalidOperationException($"A bot named '{nickname}' is in the room already: bots in a room have names of their own.");

            var seat = _lifetime.CreateNested();
            var player = new Player(nickname);
            var character = RandomCharacter();
            _seats.Add(player, seat);
            seat.Lifetime.OnTermination(() => _seats.Remove(player));
            _admitter.Admit(seat.Lifetime, player, character.Key, PlayerDriver.Server);
            _bots.AddLifetimed(seat.Lifetime, player, Entry(nickname, character, preset));
        }

        public void Remove(IPlayer bot) =>
            SeatOf(bot).Terminate();

        public void Change(IPlayer bot, BotPreset preset)
        {
            SeatOf(bot);
            var entry = _bots[bot];
            _bots[bot] = Entry(entry.Nickname, entry.Character, preset);
        }

        private bool IsTaken(string nickname)
        {
            foreach (var entry in _bots.Values)
                if (entry.Nickname == nickname)
                    return true;

            return false;
        }

        private LifetimeDefinition SeatOf(IPlayer bot) =>
            _seats.TryGetValue(bot, out var seat)
                ? seat
                : throw new InvalidOperationException($"'{bot.Nickname}' is no bot of this room.");

        private CharacterDescription RandomCharacter() =>
            _characters.Characters[Random.Range(0, _characters.Characters.Length)].Character;

        private static BotEntry Entry(string nickname, CharacterDescription character, BotPreset preset) =>
            new()
            {
                Nickname = nickname,
                Character = character,
                Difficulty = preset.Difficulty,
                Personality = preset.Personality
            };
    }
}

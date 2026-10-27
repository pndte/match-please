using System;
using System.Collections.Generic;

namespace Bw.UseCases.Bots.Network.Requests
{
    public readonly struct BotTable
    {
        public static readonly BotTable Empty = new(Array.Empty<BotTableEntry>());

        public readonly IReadOnlyList<BotTableEntry> Entries;

        public BotTable(IReadOnlyList<BotTableEntry> entries)
        {
            Entries = entries;
        }
    }

    public readonly struct BotTableEntry
    {
        public readonly int PlayerId;
        public readonly string Nickname;
        public readonly int DifficultyKey;
        public readonly int PersonalityKey;

        public BotTableEntry(
            int playerId,
            string nickname,
            int difficultyKey,
            int personalityKey)
        {
            PlayerId = playerId;
            Nickname = nickname;
            DifficultyKey = difficultyKey;
            PersonalityKey = personalityKey;
        }
    }
}

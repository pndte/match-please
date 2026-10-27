using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Serialization;
using Bw.UseCases.Bots.Network.Requests;
using Unity.Netcode;

namespace Bw.UseCases.Bots.Network.Codecs
{
    public struct BotTableCodec : ICodec<BotTable>
    {
        public BotTable Value
        {
            get => _value;
            set => _value = value;
        }

        private BotTable _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var count = serializer.IsWriter ? Value.Entries.Count : 0;
            serializer.SerializePacked(ref count);
            if (serializer.IsWriter)
            {
                for (var index = 0; index < count; index++)
                {
                    var entry = Value.Entries[index];
                    var playerId = entry.PlayerId;
                    var nickname = entry.Nickname;
                    var difficultyKey = entry.DifficultyKey;
                    var personalityKey = entry.PersonalityKey;
                    serializer.SerializePacked(ref playerId);
                    serializer.SerializeValue(ref nickname);
                    serializer.SerializePacked(ref difficultyKey);
                    serializer.SerializePacked(ref personalityKey);
                }

                return;
            }

            var entries = count == 0 ? Array.Empty<BotTableEntry>() : new BotTableEntry[count];
            for (var index = 0; index < count; index++)
            {
                var playerId = 0;
                var nickname = string.Empty;
                var difficultyKey = 0;
                var personalityKey = 0;
                serializer.SerializePacked(ref playerId);
                serializer.SerializeValue(ref nickname);
                serializer.SerializePacked(ref difficultyKey);
                serializer.SerializePacked(ref personalityKey);
                entries[index] = new BotTableEntry(playerId, nickname, difficultyKey, personalityKey);
            }

            Value = new BotTable(entries);
        }
    }
}

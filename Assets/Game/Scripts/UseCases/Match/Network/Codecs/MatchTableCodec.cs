using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Serialization;
using Bw.UseCases.Match.Network.Requests;
using Unity.Netcode;

namespace Bw.UseCases.Match.Network.Codecs
{
    public struct MatchTableCodec : ICodec<MatchTable>
    {
        public MatchTable Value
        {
            get => _value;
            set => _value = value;
        }

        private MatchTable _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var phase = serializer.IsWriter ? (int)Value.Period.Phase : 0;
            var endTick = serializer.IsWriter ? Value.Period.EndTick : 0;
            var count = serializer.IsWriter ? Value.Entries.Count : 0;

            serializer.SerializePacked(ref phase);
            serializer.SerializePacked(ref endTick);
            serializer.SerializePacked(ref count);

            if (serializer.IsWriter)
            {
                for (var index = 0; index < count; index++)
                {
                    var entry = Value.Entries[index];
                    var clientId = entry.ClientId;
                    var nickname = entry.Nickname;
                    var figure = entry.Figure;
                    var kills = entry.Score.Kills;
                    var deaths = entry.Score.Deaths;
                    var damage = entry.Score.Damage;
                    serializer.SerializePacked(ref clientId);
                    serializer.SerializeValue(ref nickname);
                    serializer.SerializeValue(ref figure);
                    serializer.SerializePacked(ref kills);
                    serializer.SerializePacked(ref deaths);
                    serializer.SerializePacked(ref damage);
                }

                return;
            }

            if (phase < (int)MatchPhase.Starting || phase > (int)MatchPhase.Ended)
                throw new InvalidOperationException($"A match table came with an unknown phase {phase.ToString()}.");

            var entries = count == 0 ? Array.Empty<MatchEntry>() : new MatchEntry[count];
            for (var index = 0; index < count; index++)
            {
                var clientId = 0UL;
                var nickname = string.Empty;
                var figure = string.Empty;
                var kills = 0;
                var deaths = 0;
                var damage = 0;
                serializer.SerializePacked(ref clientId);
                serializer.SerializeValue(ref nickname);
                serializer.SerializeValue(ref figure);
                serializer.SerializePacked(ref kills);
                serializer.SerializePacked(ref deaths);
                serializer.SerializePacked(ref damage);
                entries[index] = new MatchEntry(clientId, nickname, figure, new Score(kills, deaths, damage));
            }

            Value = new MatchTable(new MatchPeriod((MatchPhase)phase, endTick), entries);
        }
    }
}

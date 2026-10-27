using Bw.Entities.Network;
using Bw.Entities.Network.Serialization;
using Bw.UseCases.Bots.Network.Requests;
using Unity.Netcode;

namespace Bw.UseCases.Bots.Network.Codecs
{
    public struct BotPresetChangeCodec : ICodec<BotPresetChange>
    {
        public BotPresetChange Value
        {
            get => _value;
            set => _value = value;
        }

        private BotPresetChange _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var playerId = Value.PlayerId;
            var difficultyKey = Value.DifficultyKey;
            var personalityKey = Value.PersonalityKey;
            serializer.SerializePacked(ref playerId);
            serializer.SerializePacked(ref difficultyKey);
            serializer.SerializePacked(ref personalityKey);
            Value = new BotPresetChange(playerId, difficultyKey, personalityKey);
        }
    }
}

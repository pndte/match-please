using Bw.Entities.Network;
using Bw.Entities.Network.Serialization;
using Unity.Netcode;

namespace Bw.UseCases.Bots.Network.Codecs
{
    public struct RoomBotCodec : ICodec<RoomBot>
    {
        public RoomBot Value
        {
            get => _value;
            set => _value = value;
        }

        private RoomBot _value;

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var nickname = serializer.IsWriter ? Value.Nickname : string.Empty;
            var difficultyKey = Value.DifficultyKey;
            var personalityKey = Value.PersonalityKey;
            serializer.SerializeValue(ref nickname);
            serializer.SerializePacked(ref difficultyKey);
            serializer.SerializePacked(ref personalityKey);
            Value = new RoomBot(nickname, difficultyKey, personalityKey);
        }
    }
}

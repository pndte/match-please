using Bw.Entities.Network.Serialization;
using Bw.UseCases.Match.Network.Requests;
using Unity.Netcode;

namespace Bw.UseCases.Match.Network.Codecs
{
    public static class PlayerTagSerializer
    {
        public static void SerializeTag<T>(this BufferSerializer<T> serializer, ref PlayerTag tag) where T : IReaderWriter
        {
            var id = tag.Id;
            var nickname = serializer.IsWriter ? tag.Nickname : string.Empty;

            serializer.SerializePacked(ref id);
            serializer.SerializeValue(ref nickname);
            tag = new PlayerTag(id, nickname);
        }
    }
}

using Bw.Entities.Network;
using Unity.Netcode;

namespace Bw.UseCases
{
    public struct OwnershipDto
    {
        public readonly ulong RecipientClientId;
        public readonly bool IsOwner;

        public OwnershipDto(ulong recipientClientId, bool isOwner)
        {
            IsOwner = isOwner;
            RecipientClientId = recipientClientId;
        }
    }

    public struct OwnershipDtoCodec : ICodec<OwnershipDto>
    {
        public OwnershipDto Value { get; set; }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var clientId =  Value.RecipientClientId;
            var isOwner = Value.IsOwner;
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref isOwner);
            
            Value = new OwnershipDto(clientId, isOwner);
        }
    }
}
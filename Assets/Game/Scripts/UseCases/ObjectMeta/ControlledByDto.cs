using Bw.Entities.Network;
using Unity.Netcode;

namespace Bw.UseCases
{
    public struct ControlledByDto
    {
        public readonly ulong RecipientClientId;
        public readonly bool Mine;

        public ControlledByDto(ulong recipientClientId, bool mine)
        {
            RecipientClientId = recipientClientId;
            Mine = mine;
        }
    }
    
    public struct ControlledByDtoCodec : ICodec<ControlledByDto>
    {
        public ControlledByDto Value { get; set; }
        
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var clientId =  Value.RecipientClientId;
            var mine = Value.Mine;
            serializer.SerializeValue(ref clientId);
            serializer.SerializeValue(ref mine);
            
            Value = new ControlledByDto(clientId, mine);
        }
    }
}
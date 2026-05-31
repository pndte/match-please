using Bw.Entities;
using Bw.Entities.Network;
using Unity.Netcode;

namespace Bw.UseCases
{
    /// <summary>
    /// Server: <see cref="RecipientClient"/> + flag. Wire: <see cref="Value"/> only (routing via <see cref="TargetedBoolCodecRouting"/>).
    /// </summary>
    public struct TargetedBool
    {
        public readonly IClient RecipientClient;
        public readonly bool Value;

        public TargetedBool(IClient recipientClient, bool boolean)
        {
            RecipientClient = recipientClient;
            Value = boolean;
        }
    }

    public struct TargetedBoolCodec : ICodec<TargetedBool>
    {
        public TargetedBool Value { get; set; }

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            var boolean = Value.Value;
            serializer.SerializeValue(ref boolean);
            Value = new TargetedBool(null, boolean);
        }
    }

    public sealed class TargetedBoolCodecRouting : CodecTargetRouting<TargetedBoolCodec>
    {
        public override IClient GetTargetClientFromCodec(TargetedBoolCodec codec) =>
            codec.Value.RecipientClient;
    }
}

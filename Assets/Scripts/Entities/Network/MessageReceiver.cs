using Bw.Entities.Network.Variables;
using Unity.Netcode;

namespace Bw.Entities.Network
{
    public interface IMessageReceiver { }

    public interface IMessageReceiver<T> : IMessageReceiver
    {
        void Receive(ref FastBufferReader reader, INetProperty<T> property);
    }

    public sealed class MessageReceiver<TValue, TCodec> : IMessageReceiver<TValue>
        where TCodec : struct, INetworkSerializable, ICodec<TValue>
    {
        public void Receive(ref FastBufferReader reader, INetProperty<TValue> property)
        {
            reader.ReadNetworkSerializable(out TCodec codec);
            property.Value = codec.Value;
        }
    }
}
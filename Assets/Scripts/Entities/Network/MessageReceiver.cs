using Bw.Entities.Network.Variables;
using Unity.Netcode;

namespace Bw.Entities.Network
{
    public interface IMessageReceiver
    {
    }

    public interface IMessageReceiver<T> : IMessageReceiver
    {
        void ReceiveProperty(ref FastBufferReader reader, INetProperty<T> property);
        void ReceiveSignal(ref FastBufferReader reader, INetSignal<T> signal);
    }

    public sealed class MessageReceiver<TValue, TCodec> : IMessageReceiver<TValue>
        where TCodec : struct, INetworkSerializable, ICodec<TValue>
    {
        public void ReceiveProperty(ref FastBufferReader reader, INetProperty<TValue> property)
        {
            reader.ReadNetworkSerializable(out TCodec codec);
            property.Value = codec.Value;
        }

        public void ReceiveSignal(ref FastBufferReader reader, INetSignal<TValue> signal)
        {
            reader.ReadNetworkSerializable(out TCodec codec);
            signal.ApplyFromNetwork(codec.Value);
        }
    }
}

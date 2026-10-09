using Bw.Entities.Network.Variables;
using Unity.Netcode;

namespace Bw.Entities.Network
{
    public interface IMessageReceiver
    {
    }

    internal interface IMessageReceiver<T> : IMessageReceiver
    {
        void ReceiveProperty(ref FastBufferReader reader, INetProperty<T> property);
        void ReceiveSignal(ref FastBufferReader reader, INetSignal<T> signal);
    }

    public sealed class MessageReceiver<TValue, TCodec> : IMessageReceiver<TValue>
        where TCodec : struct, INetworkSerializable, ICodec<TValue>
    {
        void IMessageReceiver<TValue>.ReceiveProperty(ref FastBufferReader reader, INetProperty<TValue> property)
        {
            reader.ReadNetworkSerializable(out TCodec codec);
            property.ApplyFromNetwork(codec.Value);
        }

        void IMessageReceiver<TValue>.ReceiveSignal(ref FastBufferReader reader, INetSignal<TValue> signal)
        {
            reader.ReadNetworkSerializable(out TCodec codec);
            signal.ApplyFromNetwork(codec.Value);
        }
    }
}

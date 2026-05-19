using Unity.Netcode;

namespace Bw.Entities.Network
{
    public interface ICodec<T> : INetworkSerializable
    {
        public T Value { get; set; }
    }
}
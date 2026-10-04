using Unity.Netcode;

namespace Bw.Entities.Network
{
    public interface ICodec : INetworkSerializable //TODO: с помощью статического анализатора запретить наследовать именно этот класс
    {
        
    }
    
    public interface ICodec<T> : ICodec
    {
        public T Value { get; set; }
    }
}

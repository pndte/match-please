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

    /// <summary>
    /// Virtual dispatch on a class — struct codec is passed without !!boxing!! (unlike !!interface!! dispatch).
    /// </summary>
    public abstract class CodecTargetRouting<TCodec> where TCodec : struct, ICodec
    {
        public abstract IClient GetTargetClientFromCodec(TCodec codec);
    }
}

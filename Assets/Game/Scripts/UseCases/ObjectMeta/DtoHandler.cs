using JetBrains.Collections.Viewable;

namespace Bw.UseCases
{
    public interface IDtoSource<T> where T: struct
    {
        public ISource<T> Value { get; }
    }

    public interface IDtoBroadcaster<T> where T : struct //TODO: move
    {
        public void Fire(T dto);
    }

    public class DtoHandler<T> : IDtoSource<T>, IDtoBroadcaster<T> where T : struct
    {
        private readonly ISignal<T> _value;

        public ISource<T> Value => _value;

        public DtoHandler(ISignal<T> value)
        {
            _value = value;
        }
        
        public void Fire(T dto)
        {
            _value.Fire(dto);
        }
    }
}
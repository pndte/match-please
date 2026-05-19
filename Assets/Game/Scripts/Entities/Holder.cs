namespace Bw.Entities
{
    public interface IHolder<out T>
    {
        public T Value { get; }
    }
}
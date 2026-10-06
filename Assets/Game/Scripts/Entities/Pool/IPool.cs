using JetBrains.Lifetimes;

namespace Bw.Entities.Pool
{
    public interface IPool<out T>
    {
        public T Resource(Lifetime lifetime);
    }
}

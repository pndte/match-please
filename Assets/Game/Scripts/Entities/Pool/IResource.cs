using JetBrains.Lifetimes;

namespace Bw.Entities.Pool
{
    public interface IResource<out T>
    {
        public T Facade(Lifetime lifetime);
    }
}

using JetBrains.Lifetimes;

namespace Bw.Entities.Pool.GameObjects
{
    public sealed class PrefabPool<T> : IPool<T>
    {
        private readonly IPool<IPrefabInstance> _instances;

        public PrefabPool(IPool<IPrefabInstance> instances)
        {
            _instances = instances;
        }

        public T Resource(Lifetime lifetime) =>
            _instances.Resource(lifetime).Facade<T>();
    }
}

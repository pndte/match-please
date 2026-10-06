using System.Collections.Generic;
using Bw.Entities.Pool;
using Zenject;

namespace Bw.Injection.Pool
{
    public sealed class PoolsPrewarm : IInitializable
    {
        private readonly List<IPrewarmable> _pools;

        public PoolsPrewarm(List<IPrewarmable> pools)
        {
            _pools = pools;
        }

        public void Initialize()
        {
            foreach (var pool in _pools)
                pool.Prewarm();
        }
    }
}

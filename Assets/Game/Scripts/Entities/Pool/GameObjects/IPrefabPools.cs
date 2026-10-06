using UnityEngine;

namespace Bw.Entities.Pool.GameObjects
{
    public interface IPrefabPools
    {
        public IPool<T> For<T>(GameObject prefab) where T : class;
    }
}

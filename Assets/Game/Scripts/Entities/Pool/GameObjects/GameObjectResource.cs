using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.Entities.Pool.GameObjects
{
    public sealed class GameObjectResource : IResource<IPrefabInstance>
    {
        private readonly GameObject _gameObject;

        public GameObjectResource(GameObject gameObject)
        {
            _gameObject = gameObject;
        }

        public IPrefabInstance Facade(Lifetime lifetime)
        {
            _gameObject.SetActive(true);
            lifetime.OnTermination(() => _gameObject.SetActive(false));
            return new PrefabInstance(lifetime, _gameObject);
        }
    }
}

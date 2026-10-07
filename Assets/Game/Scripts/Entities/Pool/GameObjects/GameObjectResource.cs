using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.Entities.Pool.GameObjects
{
    public sealed class GameObjectResource : IResource<IPrefabInstance>
    {
        private readonly GameObject _gameObject;
        private readonly Lifetime _gameObjectLifetime;

        public GameObjectResource(GameObject gameObject, Lifetime gameObjectLifetime)
        {
            _gameObject = gameObject;
            _gameObjectLifetime = gameObjectLifetime;
        }

        public IPrefabInstance Facade(Lifetime lifetime)
        {
            _gameObject.SetActive(true);
            lifetime.OnTermination(() => _gameObjectLifetime.TryExecute(() => _gameObject.SetActive(false)));
            return new PrefabInstance(lifetime, _gameObject);
        }
    }
}

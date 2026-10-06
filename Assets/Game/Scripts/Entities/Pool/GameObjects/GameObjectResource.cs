using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.Entities.Pool.GameObjects
{
    public sealed class GameObjectResource<T> : IResource<T>
    {
        private readonly GameObject _gameObject;
        private readonly IResource<T> _resource;

        public GameObjectResource(GameObject gameObject, IResource<T> resource)
        {
            _gameObject = gameObject;
            _resource = resource;
        }

        public T Facade(Lifetime lifetime)
        {
            _gameObject.SetActive(true);
            lifetime.OnTermination(() => _gameObject.SetActive(false));
            return _resource.Facade(lifetime);
        }
    }
}

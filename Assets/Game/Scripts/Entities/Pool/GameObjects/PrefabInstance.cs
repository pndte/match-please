using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.Entities.Pool.GameObjects
{
    public sealed class PrefabInstance : IPrefabInstance
    {
        private readonly Lifetime _lifetime;
        private readonly GameObject _gameObject;

        public PrefabInstance(Lifetime lifetime, GameObject gameObject)
        {
            _lifetime = lifetime;
            _gameObject = gameObject;
        }

        public T Facade<T>() =>
            _gameObject.TryGetComponent<IResource<T>>(out var resource)
                ? resource.Facade(_lifetime)
                : _gameObject.GetComponent<T>();
    }
}

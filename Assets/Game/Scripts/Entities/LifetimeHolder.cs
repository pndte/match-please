using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.Entities
{
    public class LifetimeHolder : MonoBehaviour
    {
        public Lifetime GameObjectLifetime => _gameObjectLifetimeDef.Lifetime;
        public IReadonlyProperty<Lifetime> GameObjectEnabledLifetime => _gameObjectEnabledLifetime;

        private readonly LifetimeDefinition _gameObjectLifetimeDef = new();
        private ViewableProperty<Lifetime> _gameObjectEnabledLifetime;
        private SequentialLifetimes _gameObjectEnabledLifetimes;

        private void Awake()
        {
            _gameObjectEnabledLifetimes = new SequentialLifetimes(Lifetime.Terminated);
            _gameObjectEnabledLifetime = new ViewableProperty<Lifetime>(Lifetime.Terminated);
        }

        private void OnEnable()
        {
            _gameObjectEnabledLifetime.Value = _gameObjectEnabledLifetimes.Next();
        }

        private void OnDestroy()
        {
            _gameObjectLifetimeDef.Terminate();
        }

        private void OnDisable()
        {
            _gameObjectEnabledLifetimes.TerminateCurrent();
        }
    }
}
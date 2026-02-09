using System;
using Bw.Entities.Network;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public class NetworkAmmo : NetworkLifetimedBehaviour, IAmmo
    {
        public int Max => _ammoConfig.Max;

        public ISource<int> Change => _inner.Change;
        public Maybe<int> Maybe => _inner.Maybe;

        int IViewableProperty<int>.Value
        {
            get => _inner.Value;
            set => _inner.Value = Mathf.Clamp(value, 0, Max);
        }

        int IReadonlyProperty<int>.Value => _inner.Value;

        private readonly ViewableProperty<int> _inner = new(0);
        private readonly NetworkVariable<int> _networkInner = new(0);
        private AmmoConfig _ammoConfig;

        [Inject]
        private void Construct(AmmoConfig ammoConfig)
        {
            _ammoConfig = ammoConfig;
            WhenAlive(ConnectNetworkData); // TODO: надо переносить лайфтаймы в конструктор
        }

        private void ConnectNetworkData(Lifetime lifetime)
        {
            _networkInner.Value = _ammoConfig.OnSpawnValue;
            _networkInner.ConnectTo(lifetime, _inner);
        }

        public void Advise(Lifetime lifetime, Action<int> handler)
        {
            _inner.Advise(lifetime, handler);
        }
    }
}
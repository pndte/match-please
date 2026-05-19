using System;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.Weapon.Network
{
    public class Ammo : IAmmo
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

        private readonly IViewableProperty<int> _inner;
        private readonly AmmoConfig _ammoConfig;

        public Ammo(AmmoConfig ammoConfig, IViewableProperty<int> inner)
        {
            _ammoConfig = ammoConfig;
            _inner = inner;
        }

        public void Advise(Lifetime lifetime, Action<int> handler)
        {
            _inner.Advise(lifetime, handler);
        }
    }
}
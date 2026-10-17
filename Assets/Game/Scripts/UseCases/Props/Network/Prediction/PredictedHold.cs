using System;
using Bw.Entities.Extensions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props.Network.Prediction
{
    public sealed class PredictedHold : IWeaponHold
    {
        public IReadonlyProperty<Lifetime> HeldLifetime => _held;
        public bool IsReleasedEarly { get; private set; }

        private readonly ViewableProperty<Lifetime> _held = new(Lifetime.Terminated);
        private readonly SequentialLifetimes _holds;

        public PredictedHold(Lifetime lifetime, IWeaponHold network)
        {
            _holds = new SequentialLifetimes(lifetime);

            network.HeldLifetime.WhenAlive(lifetime, networkLifetime =>
            {
                Take();
                networkLifetime.OnTermination(Drop);
            });
        }

        public void Release()
        {
            if (!_held.Value.IsAlive)
                throw new InvalidOperationException("A predicted throw releases an item from the hands, and this one is not in them.");

            IsReleasedEarly = true;
            _holds.TerminateCurrent();
            _held.Value = Lifetime.Terminated;
        }

        public void Restore()
        {
            if (!IsReleasedEarly)
                throw new InvalidOperationException("Only an item released early by a predicted throw goes back to the hands.");

            IsReleasedEarly = false;
            Take();
        }

        private void Take() =>
            _held.Value = _holds.Next();

        private void Drop()
        {
            IsReleasedEarly = false;
            _holds.TerminateCurrent();
            _held.Value = Lifetime.Terminated;
        }
    }
}

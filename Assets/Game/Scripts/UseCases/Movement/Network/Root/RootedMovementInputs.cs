using System;
using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Movement.Network.Root
{
    public sealed class RootedMovementInputs : ISource<MovementInput>
    {
        private readonly ISource<MovementInput> _inputs;
        private readonly IReadonlyMovementRoot _root;
        private readonly INetworkTicks _ticks;

        public RootedMovementInputs(ISource<MovementInput> inputs, IReadonlyMovementRoot root, INetworkTicks ticks)
        {
            _inputs = inputs;
            _root = root;
            _ticks = ticks;
        }

        public void Advise(Lifetime lifetime, Action<MovementInput> handler) =>
            _inputs.Advise(lifetime, input => handler(_root.RootedAt(_ticks.Current) ? default : input));
    }
}

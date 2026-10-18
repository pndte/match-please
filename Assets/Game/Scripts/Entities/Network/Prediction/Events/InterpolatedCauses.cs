using System.Collections.Generic;
using Bw.Entities.Network.Prediction.Events.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using R3;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class InterpolatedCauses<TState> : INetResultReceiver<CausedState<TState>> where TState : struct
    {
        public ISource<CausedState<TState>> Received => _shown;

        private readonly Signal<CausedState<TState>> _shown = new();
        private readonly Queue<CausedState<TState>> _waiting = new();
        private readonly IInterpolationTicks _ticks;

        public InterpolatedCauses(Lifetime lifetime, INetResultReceiver<CausedState<TState>> received, IInterpolationTicks ticks)
        {
            _ticks = ticks;

            received.Received.Advise(lifetime, Wait);
            Observable.EveryUpdate(UnityFrameProvider.Update, lifetime).Subscribe(_ => Release());
        }

        private void Wait(CausedState<TState> state)
        {
            _waiting.Enqueue(state);
            Release();
        }

        private void Release()
        {
            while (_waiting.Count > 0 && IsShown(_waiting.Peek().Cause))
                _shown.Fire(_waiting.Dequeue());
        }

        private bool IsShown(StateCause cause) =>
            !cause.IsAction || _ticks.InterpolationTick >= cause.Action.Tick;
    }
}

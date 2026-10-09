using System;
using System.Collections.Generic;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class PredictionTargets<TEffect> : IPredictionTargets<TEffect> where TEffect : struct
    {
        private readonly Dictionary<object, IPredictionTarget<TEffect>> _byTarget = new(new SameObjectComparer());
        private readonly Dictionary<ulong, IPredictionTarget<TEffect>> _byNetworkObjectId = new();

        public void AddLifetimed<TState>(Lifetime lifetime, IReadonlyAppliedState<TState> target, ulong networkObjectId, IPredictionTarget<TEffect> predictionTarget)
            where TState : struct
        {
            if (_byTarget.ContainsKey(target) || _byNetworkObjectId.ContainsKey(networkObjectId))
                throw new InvalidOperationException("A target is registered once at a time, and a network object holds one target.");

            _byTarget.Add(target, predictionTarget);
            _byNetworkObjectId.Add(networkObjectId, predictionTarget);
            lifetime.OnTermination(() =>
            {
                _byTarget.Remove(target);
                _byNetworkObjectId.Remove(networkObjectId);
            });
        }

        public bool TryGet<TTarget>(TTarget target, out IPredictionTarget<TEffect> predictionTarget) where TTarget : class =>
            _byTarget.TryGetValue(target, out predictionTarget);

        public bool TryGet(ulong networkObjectId, out IPredictionTarget<TEffect> predictionTarget) =>
            _byNetworkObjectId.TryGetValue(networkObjectId, out predictionTarget);
    }
}

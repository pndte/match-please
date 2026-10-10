using System;
using System.Collections.Generic;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class PredictionTargets<TEffect> : IPredictionTargets<TEffect> where TEffect : struct
    {
        private readonly Dictionary<object, Target> _byTarget = new(new SameObjectComparer());
        private readonly Dictionary<ulong, Target> _byNetworkObjectId = new();

        public void AddLifetimed<TTarget>(Lifetime lifetime, TTarget target, ulong networkObjectId, IPredictionTarget<TEffect> part) where TTarget : class
        {
            var known = _byTarget.TryGetValue(target, out var entry);
            if (known && entry.NetworkObjectId != networkObjectId)
                throw new InvalidOperationException(
                    $"Every part of a target lives on the target's network object: object {networkObjectId.ToString()} tried to add a part to the target of object {entry.NetworkObjectId.ToString()}.");

            if (!known && _byNetworkObjectId.ContainsKey(networkObjectId))
                throw new InvalidOperationException(
                    $"A network object holds one target: object {networkObjectId.ToString()} already holds another one, so its parts must name that target.");

            if (known && entry.Parts.Contains(part))
                throw new InvalidOperationException("A part is registered once at a time: the same part was added to its target twice.");

            if (!known)
            {
                entry = new Target(networkObjectId);
                _byTarget.Add(target, entry);
                _byNetworkObjectId.Add(networkObjectId, entry);
            }

            entry.Parts.Add(part);
            lifetime.OnTermination(() => Remove(target, entry, part));
        }

        public bool TryGet<TTarget>(TTarget target, out IPredictionTarget<TEffect> predictionTarget) where TTarget : class
        {
            var found = _byTarget.TryGetValue(target, out var entry);
            predictionTarget = entry;
            return found;
        }

        public bool TryGet(ulong networkObjectId, out IPredictionTarget<TEffect> predictionTarget)
        {
            var found = _byNetworkObjectId.TryGetValue(networkObjectId, out var entry);
            predictionTarget = entry;
            return found;
        }

        private void Remove(object target, Target entry, IPredictionTarget<TEffect> part)
        {
            entry.Parts.Remove(part);
            if (entry.Parts.Count > 0)
                return;

            _byTarget.Remove(target);
            _byNetworkObjectId.Remove(entry.NetworkObjectId);
        }

        private sealed class Target : IPredictionTarget<TEffect>
        {
            public readonly ulong NetworkObjectId;
            public readonly List<IPredictionTarget<TEffect>> Parts = new();

            public Target(ulong networkObjectId)
            {
                NetworkObjectId = networkObjectId;
            }

            public void Show(PredictedEffect<TEffect> effect)
            {
                for (var index = 0; index < Parts.Count; index++)
                    Parts[index].Show(effect);
            }

            public void Confirm(ActionId action, TEffect actual)
            {
                for (var index = 0; index < Parts.Count; index++)
                    Parts[index].Confirm(action, actual);
            }

            public void Reject(ActionId action)
            {
                for (var index = 0; index < Parts.Count; index++)
                    Parts[index].Reject(action);
            }
        }
    }
}

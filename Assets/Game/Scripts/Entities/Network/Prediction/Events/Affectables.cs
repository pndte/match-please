using System;
using System.Collections.Generic;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class Affectables<TEffect> : IAffectables<TEffect> where TEffect : struct
    {
        private readonly Dictionary<object, Entry> _entries = new(new SameObjectComparer());

        public void AddLifetimed<TState>(Lifetime lifetime, IReadonlyAppliedState<TState> target, ulong networkObjectId, IAffectable<TEffect> affectable)
            where TState : struct
        {
            if (_entries.ContainsKey(target))
                throw new InvalidOperationException("A target is registered once at a time.");

            _entries.Add(target, new Entry(networkObjectId, affectable));
            lifetime.OnTermination(() => _entries.Remove(target));
        }

        public bool TryGet<TTarget>(TTarget target, out ulong networkObjectId, out IAffectable<TEffect> affectable) where TTarget : class
        {
            var found = _entries.TryGetValue(target, out var entry);
            networkObjectId = entry.NetworkObjectId;
            affectable = entry.Affectable;
            return found;
        }

        private readonly struct Entry
        {
            public readonly ulong NetworkObjectId;
            public readonly IAffectable<TEffect> Affectable;

            public Entry(ulong networkObjectId, IAffectable<TEffect> affectable)
            {
                NetworkObjectId = networkObjectId;
                Affectable = affectable;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Events
{
    public sealed class Affectables<TEffect> : IAffectables<TEffect> where TEffect : struct
    {
        private readonly Dictionary<object, Target> _targets = new(new SameObjectComparer());

        public void AddLifetimed<TTarget>(Lifetime lifetime, TTarget target, ulong networkObjectId, IAffectable<TEffect> part) where TTarget : class
        {
            var known = _targets.TryGetValue(target, out var entry);
            if (known && entry.NetworkObjectId != networkObjectId)
                throw new InvalidOperationException(
                    $"Every part of a target lives on the target's network object: object {networkObjectId.ToString()} tried to add a part to the target of object {entry.NetworkObjectId.ToString()}.");

            if (known && entry.Parts.Contains(part))
                throw new InvalidOperationException("A part is registered once at a time: the same part was added to its target twice.");

            if (!known)
            {
                entry = new Target(networkObjectId);
                _targets.Add(target, entry);
            }

            entry.Parts.Add(part);
            lifetime.OnTermination(() => Remove(target, entry, part));
        }

        public bool TryGet<TTarget>(TTarget target, out ulong networkObjectId, out IAffectable<TEffect> affectable) where TTarget : class
        {
            var found = _targets.TryGetValue(target, out var entry);
            networkObjectId = found ? entry.NetworkObjectId : 0UL;
            affectable = entry;
            return found;
        }

        private void Remove(object target, Target entry, IAffectable<TEffect> part)
        {
            entry.Parts.Remove(part);
            if (entry.Parts.Count == 0)
                _targets.Remove(target);
        }

        private sealed class Target : IAffectable<TEffect>
        {
            public readonly ulong NetworkObjectId;
            public readonly List<IAffectable<TEffect>> Parts = new();

            public Target(ulong networkObjectId)
            {
                NetworkObjectId = networkObjectId;
            }

            public void Affect(ActionId action, TEffect effect)
            {
                for (var index = 0; index < Parts.Count; index++)
                    Parts[index].Affect(action, effect);
            }
        }
    }
}

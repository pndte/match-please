using System;
using System.Collections.Generic;
using JetBrains.Lifetimes;

namespace Bw.Entities.Loop
{
    public class DynamicGroup : IUpdateGroup, ISubscrible // TODO:
    {
        public IReadOnlyList<IUpdatable> Subscribers { get; }

        public DynamicGroup(IReadOnlyList<IUpdatable> subscribers)
        {
            Subscribers = subscribers;
        }

        public void Subscribe(Lifetime lifetime, Action action)
        {
            throw new NotImplementedException();
        }
    }
}
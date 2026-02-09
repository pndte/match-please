using System.Collections.Generic;

namespace Bw.Entities.Loop
{
    public class StaticGroup : IUpdateGroup
    {
        public IReadOnlyList<IUpdatable> Subscribers { get; }
        
        public StaticGroup(IReadOnlyList<IUpdatable> subscribers)
        {
            Subscribers = subscribers;
        }

        public StaticGroup(params IUpdatable[] subscribers)
        {
            Subscribers = new List<IUpdatable>(subscribers);
        }
    }
}
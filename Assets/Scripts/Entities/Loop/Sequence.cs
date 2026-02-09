using Bw.Entities.Extensions;

namespace Bw.Entities.Loop
{
    class Sequence : IUpdatable
    {
        private readonly IUpdateGroup _group;

        public Sequence(IUpdateGroup group)
        {
            _group = group;
        }

        public Sequence(params IUpdatable[] subscribers)
        {
            _group = new StaticGroup(subscribers);
        }

        public void Update()
        {
            _group.Subscribers.ForEach(updatable => updatable.Update());
        }
    }
}
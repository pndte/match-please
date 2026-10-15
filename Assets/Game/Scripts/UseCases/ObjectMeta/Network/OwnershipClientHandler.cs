using Bw.Entities;
using Bw.Entities.Network.Variables;
using JetBrains.Lifetimes;

namespace Bw.UseCases.ObjectMeta.Network
{
    public sealed class OwnershipClientHandler
    {
        private readonly SequentialLifetimes _owned;
        private readonly IOwnership _ownership;

        public OwnershipClientHandler(
            Lifetime lifetime,
            INetResultReceiver<bool> mine,
            IOwnership ownership)
        {
            _owned = new SequentialLifetimes(lifetime);
            _ownership = ownership;

            mine.Received.Advise(lifetime, Receive);
        }

        private void Receive(bool mine)
        {
            if (mine)
                _ownership.SetMine(_owned.Next());
            else
                _owned.TerminateCurrent();
        }
    }
}

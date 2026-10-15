using Bw.Entities;
using Bw.Entities.Network.Variables;
using JetBrains.Lifetimes;

namespace Bw.UseCases.ObjectMeta.Network
{
    public sealed class ControlledByClientHandler
    {
        private readonly SequentialLifetimes _controlled;
        private readonly IControlledBy _controlledBy;

        public ControlledByClientHandler(
            Lifetime lifetime,
            INetResultReceiver<bool> me,
            IControlledBy controlledBy)
        {
            _controlled = new SequentialLifetimes(lifetime);
            _controlledBy = controlledBy;

            me.Received.Advise(lifetime, Receive);
        }

        private void Receive(bool me)
        {
            if (me)
                _controlledBy.SetMe(_controlled.Next());
            else
                _controlled.TerminateCurrent();
        }
    }
}

using Bw.Entities.Network;
using Bw.Entities.Network.Variables;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Match.Network
{
    public sealed class LocalPlayerReceiver
    {
        private readonly SequentialLifetimes _told;
        private readonly ILocalPlayer _localPlayer;

        public LocalPlayerReceiver(
            Lifetime lifetime,
            INetResultReceiver<int> told,
            ILocalPlayer localPlayer)
        {
            _told = new SequentialLifetimes(lifetime);
            _localPlayer = localPlayer;

            told.Received.Advise(lifetime, Receive);
        }

        private void Receive(int id) =>
            _localPlayer.Set(_told.Next(), id);
    }
}

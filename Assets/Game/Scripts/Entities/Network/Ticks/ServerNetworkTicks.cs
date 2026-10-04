using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;

namespace Bw.Entities.Network.Ticks
{
    public sealed class ServerNetworkTicks : INetworkTicks
    {
        public float Duration { get; private set; }
        public int Current { get; private set; }
        public ISource<int> Ticked => _ticked;

        private readonly Signal<int> _ticked = new();
        private NetworkManager _network;

        public ServerNetworkTicks(Lifetime lifetime, INetworkHolder networkHolder)
        {
            networkHolder.NetworkManager.AdviseNotNull(lifetime, network =>
            {
                _network = network;
                Duration = 1f / network.NetworkConfig.TickRate;

                var tickSystem = network.NetworkTickSystem;
                tickSystem.Tick += OnTick;
                lifetime.OnTermination(() => tickSystem.Tick -= OnTick);
            });
        }

        private void OnTick()
        {
            Current = _network.ServerTime.Tick;
            _ticked.Fire(Current);
        }
    }
}

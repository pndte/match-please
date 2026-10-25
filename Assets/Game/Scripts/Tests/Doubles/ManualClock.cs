using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.Doubles
{
    public sealed class ManualClock : IWorldClock, INetworkTicks
    {
        public ISource<int> Ticked => _ticked;
        public int Current { get; private set; }
        public float Duration { get; }

        private readonly Signal<int> _ticked = new();

        public ManualClock(float duration = 1f / 30f)
        {
            Duration = duration;
        }

        public void Tick(int tick)
        {
            Current = tick;
            _ticked.Fire(tick);
        }

        public void Advance() =>
            Tick(Current + 1);

        public void Advance(int ticks)
        {
            for (var index = 0; index < ticks; index++)
                Advance();
        }
    }
}

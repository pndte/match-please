using System;
using JetBrains.Collections.Viewable;

namespace Bw.Entities.Network.Ticks
{
    internal sealed class TickPhaseSignals
    {
        private static readonly TickPhase[] Phases = (TickPhase[])Enum.GetValues(typeof(TickPhase));

        private readonly Signal<int>[] _signals = new Signal<int>[Phases.Length];

        public TickPhaseSignals()
        {
            for (var index = 0; index < Phases.Length; index++)
            {
                if ((int)Phases[index] != index)
                    throw new InvalidOperationException(
                        $"Tick phase number {index.ToString()} has value {((int)Phases[index]).ToString()}: phases must be numbered 0, 1, 2… in execution order.");

                _signals[index] = new Signal<int>();
            }
        }

        public ISource<int> Of(TickPhase phase) =>
            _signals[(int)phase];

        public void Fire(int tick)
        {
            for (var index = 0; index < _signals.Length; index++)
                _signals[index].Fire(tick);
        }
    }
}

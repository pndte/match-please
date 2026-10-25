#if UNITY_EDITOR
using System.Globalization;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.Entities.Network.Ticks
{
    public sealed class InputDelayLog
    {
        private readonly INetworkTicks _clock;

        public InputDelayLog(Lifetime lifetime, IInputDelay delay, INetworkTicks clock)
        {
            _clock = clock;

            delay.Ticks.Advise(lifetime, Report);
        }

        private void Report(int ticks) =>
            Debug.Log($"[Input] own input applies {ticks.ToString()} ticks after it is sampled ({(ticks * _clock.Duration * 1000f).ToString("0", CultureInfo.InvariantCulture)} ms).");
    }
}
#endif

using System.Globalization;
using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class InputGapLog<TInput> where TInput : struct
    {
        private readonly InputGaps _gaps;
        private readonly NetworkTicksConfig _config;
        private readonly NetworkObject _networkObject;

        private int _ticks;
        private int _late;
        private int _lateTicks;
        private int _early;
        private int _recovered;
        private int _substituted;

        public InputGapLog(
            Lifetime lifetime,
            INetworkTicks ticks,
            NetworkTicksConfig config,
            PredictionInputBuffer<TInput> buffer,
            NetworkObject networkObject)
        {
            _gaps = buffer.Gaps;
            _config = config;
            _networkObject = networkObject;

            ticks.Ticked(TickPhase.Default).Advise(lifetime, _ => ReportWhenDue());
        }

        private void ReportWhenDue()
        {
            if (++_ticks < _config.GapReportIntervalTicks)
                return;

            var late = _gaps.Late - _late;
            var lateTicks = _gaps.LateTicks - _lateTicks;
            var early = _gaps.Early - _early;
            var recovered = _gaps.Recovered - _recovered;
            var substituted = _gaps.Substituted - _substituted;
            _ticks = 0;
            _late = _gaps.Late;
            _lateTicks = _gaps.LateTicks;
            _early = _gaps.Early;
            _recovered = _gaps.Recovered;
            _substituted = _gaps.Substituted;
            if (late == 0 && early == 0 && recovered == 0 && substituted == 0)
                return;

            Debug.LogWarning(
                $"[Input] {_networkObject.name} #{_networkObject.NetworkObjectId.ToString()} {typeof(TInput).Name}, last {_config.GapReportIntervalTicks.ToString()} ticks: "
                + $"{late.ToString()} inputs came after their tick and were dropped"
                + (late == 0 ? "" : $" ({(lateTicks / (float)late).ToString("0.0", CultureInfo.InvariantCulture)} ticks late on average)")
                + $", {early.ToString()} came more than {_config.MaxBufferedInputTicks.ToString()} ticks early and were dropped"
                + $", {recovered.ToString()} missing inputs came from the next packet's copy, {substituted.ToString()} ticks ran on a repeated input.");
        }
    }
}

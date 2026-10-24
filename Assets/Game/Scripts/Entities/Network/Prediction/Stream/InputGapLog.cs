#if UNITY_EDITOR
using System;
using System.Globalization;
using Bw.Entities.Network.Rollback;
using Bw.Entities.Network.Ticks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using Unity.Netcode;
using UnityEngine;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class InputGapLog<TInput> where TInput : struct
    {
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
            IWorldStep worldStep,
            NetworkTicksConfig config,
            PredictionInputBuffer<TInput> buffer,
            NetworkObject networkObject)
        {
            _config = config;
            _networkObject = networkObject;

            buffer.Gaps.Advise(lifetime, Count);
            worldStep.Stepped(TickPhase.Publish).Advise(lifetime, _ => ReportWhenDue());
        }

        private void Count(InputGap gap)
        {
            switch (gap.Kind)
            {
                case InputGapKind.Late:
                    _late++;
                    _lateTicks += gap.LateTicks;
                    break;
                case InputGapKind.Early:
                    _early++;
                    break;
                case InputGapKind.Recovered:
                    _recovered++;
                    break;
                case InputGapKind.Substituted:
                    _substituted++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(gap));
            }
        }

        private void ReportWhenDue()
        {
            if (++_ticks < _config.GapReportIntervalTicks)
                return;

            if (_late > 0 || _early > 0 || _recovered > 0 || _substituted > 0)
                Debug.LogWarning(
                    $"[Input] {_networkObject.name} #{_networkObject.NetworkObjectId.ToString()} {typeof(TInput).Name}, last {_config.GapReportIntervalTicks.ToString()} ticks: "
                    + $"{_late.ToString()} inputs came after their tick and were dropped"
                    + (_late == 0 ? "" : $" ({(_lateTicks / (float)_late).ToString("0.0", CultureInfo.InvariantCulture)} ticks late on average)")
                    + $", {_early.ToString()} came more than {_config.MaxBufferedInputTicks.ToString()} ticks early and were dropped"
                    + $", {_recovered.ToString()} missing inputs came from the next packet's copy, {_substituted.ToString()} ticks ran on a repeated input.");

            _ticks = 0;
            _late = 0;
            _lateTicks = 0;
            _early = 0;
            _recovered = 0;
            _substituted = 0;
        }
    }
}
#endif

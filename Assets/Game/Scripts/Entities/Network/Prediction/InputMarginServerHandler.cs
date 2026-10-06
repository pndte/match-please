using System;
using Bw.Entities.Network.Prediction.Requests;
using Bw.Entities.Network.Ticks;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction
{
    public sealed class InputMarginServerHandler<TInput> where TInput : struct
    {
        private readonly INetworkTicks _ticks;
        private readonly IInputMarginResult _marginResult;
        private readonly NetworkTicksConfig _config;

        private int _minMargin;
        private bool _measured;
        private int _ticksSinceReport;

        public InputMarginServerHandler(
            Lifetime lifetime,
            INetworkTicks ticks,
            IPredictionInputRequest<TInput> inputRequest,
            IInputMarginResult marginResult,
            NetworkTicksConfig config)
        {
            _ticks = ticks;
            _marginResult = marginResult;
            _config = config;

            inputRequest.Requested.Advise(lifetime, request => Measure(request.Tick - _ticks.Current - 1));
            ticks.Ticked(TickPhase.Default).Advise(lifetime, _ => ReportWhenDue());
        }

        private void Measure(int margin)
        {
            _minMargin = _measured ? Math.Min(_minMargin, margin) : margin;
            _measured = true;
        }

        private void ReportWhenDue()
        {
            if (++_ticksSinceReport < _config.MarginReportIntervalTicks)
                return;

            _ticksSinceReport = 0;
            if (!_measured)
                return;

            _measured = false;
            _marginResult.Received.Fire(_minMargin);
        }
    }
}

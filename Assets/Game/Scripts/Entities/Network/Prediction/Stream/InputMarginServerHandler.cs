using System;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Network.Variables;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Prediction.Stream
{
    public sealed class InputMarginServerHandler<TInput> where TInput : struct
    {
        private readonly INetworkTicks _ticks;
        private readonly INetResultSender<int> _margin;
        private readonly NetworkTicksConfig _config;

        private int _minMargin;
        private bool _measured;
        private int _ticksSinceReport;

        public InputMarginServerHandler(
            Lifetime lifetime,
            INetworkTicks ticks,
            INetRequestReceiver<TickedInput<TInput>> input,
            INetResultSender<int> margin,
            NetworkTicksConfig config)
        {
            _ticks = ticks;
            _margin = margin;
            _config = config;

            input.Received.Advise(lifetime, request => Measure(request.Tick - _ticks.Current - 1));
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
            _margin.Broadcast(_minMargin);
        }
    }
}

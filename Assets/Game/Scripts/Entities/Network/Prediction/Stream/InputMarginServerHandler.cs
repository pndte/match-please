using System;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Rollback;
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

        public InputMarginServerHandler(
            Lifetime lifetime,
            INetworkTicks ticks,
            IWorldStep worldStep,
            INetRequestReceiver<TickedInput<TInput>> input,
            INetResultSender<int> margin,
            NetworkTicksConfig config)
        {
            _ticks = ticks;
            _margin = margin;
            _config = config;

            input.Received.Advise(lifetime, request => Measure(request.Tick - _ticks.Current - 1));
            worldStep.Stepped(TickPhase.Publish).Advise(lifetime, ReportWhenDue);
        }

        private void Measure(int margin)
        {
            _minMargin = _measured ? Math.Min(_minMargin, margin) : margin;
            _measured = true;
        }

        private void ReportWhenDue(int tick)
        {
            if (tick % _config.MarginReportIntervalTicks != 0 || !_measured)
                return;

            _measured = false;
            _margin.Broadcast(_minMargin);
        }
    }
}

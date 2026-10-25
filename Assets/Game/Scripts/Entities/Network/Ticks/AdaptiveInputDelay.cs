using Bw.Entities.Network.Rollback;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Ticks
{
    public sealed class AdaptiveInputDelay : IInputDelay //TODO: задержку выбирает сам клиент по своему опережению: изменённый клиент может держать её нулевой, и остальные дольше угадывают его ввод; в Brawlhalla её назначает сервер по среднему пингу игрока
    {
        private const float WholeTickRounding = 0.5f;

        public IReadonlyProperty<int> Ticks => _ticks;

        private readonly ViewableProperty<int> _ticks;
        private readonly INetworkTicks _clock;
        private readonly IInputLead _lead;
        private readonly NetworkTicksConfig _config;

        private int _wish;
        private int _wishedTicks;

        public AdaptiveInputDelay(
            Lifetime lifetime,
            IWorldStep worldStep,
            INetworkTicks clock,
            IInputLead lead,
            NetworkTicksConfig config)
        {
            _ticks = new ViewableProperty<int>(config.MinInputDelayTicks);
            _clock = clock;
            _lead = lead;
            _config = config;

            worldStep.Stepped(TickPhase.Prepare).Advise(lifetime, _ => Settle());
        }

        private void Settle()
        {
            var step = WantedStep();
            _wishedTicks = step == _wish ? _wishedTicks + 1 : 1;
            _wish = step;
            if (step == 0 || _wishedTicks * _clock.Duration < _config.InputDelayHoldSeconds)
                return;

            _wishedTicks = 0;
            _ticks.Value += step;
        }

        private int WantedStep()
        {
            var delay = _ticks.Value;
            var earlyTicks = _lead.MarginTicks + 1f + WholeTickRounding;
            var roundTripTicks = _lead.Ticks + delay - earlyTicks;
            var wanted = roundTripTicks - _config.FreeRoundTripSeconds / _clock.Duration;
            if (wanted > delay + 0.5f + _config.InputDelayHysteresisTicks && delay < _config.MaxInputDelayTicks)
                return 1;

            if (wanted < delay - 0.5f - _config.InputDelayHysteresisTicks && delay > _config.MinInputDelayTicks)
                return -1;

            return 0;
        }
    }
}

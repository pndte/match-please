using Bw.Entities.Network.Prediction.Stream;
using Bw.UseCases.Combat.Hands.Network.Requests;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class HandsControlsSampler : IInputSampler<HandsControls>
    {
        private readonly CursorAim _aim;
        private readonly TriggerSampler _trigger;
        private readonly ReloadSampler _reload;
        private readonly DrawControlSampler _choices;

        public HandsControlsSampler(
            CursorAim aim,
            TriggerSampler trigger,
            ReloadSampler reload,
            DrawControlSampler choices)
        {
            _aim = aim;
            _trigger = trigger;
            _reload = reload;
            _choices = choices;
        }

        public HandsControls Sample()
        {
            var choice = _choices.Sample();
            return new(_aim.Sample(), _trigger.Sample(choice), _reload.Sample(), choice);
        }
    }
}

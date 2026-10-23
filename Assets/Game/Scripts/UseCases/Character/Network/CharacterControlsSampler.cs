using Bw.Entities.Network.Prediction.Stream;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Movement.Network.Requests;

namespace Bw.UseCases.Character.Network
{
    public sealed class CharacterControlsSampler : IInputSampler<CharacterControls>
    {
        private readonly IInputSampler<MovementControls> _movement;
        private readonly IInputSampler<HandsControls> _hands;
        private readonly GuardStanceSampler _guard;

        public CharacterControlsSampler(
            IInputSampler<MovementControls> movement,
            IInputSampler<HandsControls> hands,
            GuardStanceSampler guard)
        {
            _movement = movement;
            _hands = hands;
            _guard = guard;
        }

        public CharacterControls Sample() =>
            new(_movement.Sample(), _hands.Sample(), _guard.Sample());
    }
}

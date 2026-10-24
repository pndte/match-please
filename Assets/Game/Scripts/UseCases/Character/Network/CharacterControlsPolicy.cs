using Bw.Entities.Network.Prediction.Stream;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Movement.Network.Requests;

namespace Bw.UseCases.Character.Network
{
    public sealed class CharacterControlsPolicy : IInputPolicy<CharacterControls>
    {
        private readonly IInputPolicy<MovementControls> _movement;
        private readonly IInputPolicy<HandsControls> _hands;

        public CharacterControlsPolicy(IInputPolicy<MovementControls> movement, IInputPolicy<HandsControls> hands)
        {
            _movement = movement;
            _hands = hands;
        }

        public bool IsValid(CharacterControls controls) =>
            _movement.IsValid(controls.Movement) && _hands.IsValid(controls.Hands) && float.IsFinite(controls.Guard.Aim);

        public CharacterControls CaughtUp(
            CharacterControls controls,
            CharacterControls lastControls,
            CharacterControls heard,
            int heardTicksAgo) =>
            new(
                _movement.CaughtUp(controls.Movement, lastControls.Movement, heard.Movement, heardTicksAgo),
                _hands.CaughtUp(controls.Hands, lastControls.Hands, heard.Hands, heardTicksAgo),
                controls.Guard);
    }
}

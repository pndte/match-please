using Bw.Entities.Network.Prediction.Stream;
using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Hands.Network;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Movement.Network.Requests;

namespace Bw.UseCases.Bots.Network
{
    public sealed class BotCharacterSampler : IInputSampler<CharacterControls>
    {
        private readonly IReadonlyBotGamepad _gamepad;
        private readonly HandsControlsForm _hands;

        public BotCharacterSampler(IReadonlyBotGamepad gamepad, HandsControlsForm hands)
        {
            _gamepad = gamepad;
            _hands = hands;
        }

        public CharacterControls Sample()
        {
            var movement = _gamepad.Movement;
            return new CharacterControls(
                new MovementControls(movement.Horizontal, movement.JumpPressed, movement.JumpHeld, movement.Down),
                new HandsControls(
                    _gamepad.Aim,
                    new TriggerControls(_gamepad.TriggerPulled, 0f, 0f, default),
                    _gamepad.ReloadPressed,
                    _hands.ControlOf(_gamepad.Choice)),
                GuardStance.Lowered);
        }
    }
}

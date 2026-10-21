using Bw.Entities.Network.Prediction.Stream;
using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Movement;

namespace Bw.UseCases.Bots.Network
{
    public sealed class BotMovementSampler : IInputSampler<MovementInput>
    {
        private readonly IReadonlyBotGamepad _gamepad;

        public BotMovementSampler(IReadonlyBotGamepad gamepad)
        {
            _gamepad = gamepad;
        }

        public MovementInput Sample() =>
            _gamepad.Movement;
    }
}

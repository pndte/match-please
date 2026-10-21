using Bw.Entities.Network.Prediction.Stream;
using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Shooting.Weapon;

namespace Bw.UseCases.Bots.Network
{
    public sealed class BotWeaponSampler : IInputSampler<WeaponInput>
    {
        private readonly IReadonlyBotGamepad _gamepad;

        public BotWeaponSampler(IReadonlyBotGamepad gamepad)
        {
            _gamepad = gamepad;
        }

        public WeaponInput Sample() =>
            new(_gamepad.Aim, _gamepad.TriggerPulled, _gamepad.ReloadPressed, 0f, 0f, default);
    }
}

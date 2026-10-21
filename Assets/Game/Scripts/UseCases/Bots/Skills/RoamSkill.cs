using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Bots.Navigation;

namespace Bw.UseCases.Bots.Skills
{
    public sealed class RoamSkill : IBotSkill
    {
        private const float Forward = 0f;
        private const float Backward = 180f;

        public SkillStatus Status { get; private set; }

        private readonly BotNavigator _navigator;
        private readonly IBotGamepad _gamepad;

        public RoamSkill(BotNavigator navigator, IBotGamepad gamepad)
        {
            _navigator = navigator;
            _gamepad = gamepad;
        }

        public void Begin()
        {
            _navigator.Stop();
            Status = _navigator.TryWander() ? SkillStatus.Running : SkillStatus.Failed;
        }

        public void Perform(Perception perception)
        {
            _navigator.Steer();
            _gamepad.PressReload();
            var horizontal = _gamepad.Movement.Horizontal;
            if (horizontal != 0f)
                _gamepad.AimAt(horizontal > 0f ? Forward : Backward);

            Status = _navigator.Status switch
            {
                NavStatus.Arrived => SkillStatus.Succeeded,
                NavStatus.Lost => SkillStatus.Failed,
                _ => SkillStatus.Running
            };
        }
    }
}

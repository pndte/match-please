using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Bots.Extensions;
using Bw.UseCases.Bots.Goap;
using Bw.UseCases.Bots.Navigation;

namespace Bw.UseCases.Bots.Skills
{
    public sealed class CloseInSkill : IBotSkill
    {
        public SkillStatus Status { get; private set; }

        private readonly BotNavigator _navigator;
        private readonly IBotGamepad _gamepad;
        private readonly BotFact _until;

        public CloseInSkill(BotNavigator navigator, IBotGamepad gamepad, BotFact until)
        {
            _navigator = navigator;
            _gamepad = gamepad;
            _until = until;
        }

        public void Begin()
        {
            Status = SkillStatus.Running;
            _navigator.Stop();
        }

        public void Perform(Perception perception)
        {
            if ((perception.Facts & _until) == _until)
            {
                Finish(SkillStatus.Succeeded);
                return;
            }

            if (!perception.TryFindTarget(out var target) || _navigator.Status == NavStatus.Arrived)
            {
                Finish(SkillStatus.Failed);
                return;
            }

            _gamepad.AimAt(perception.Eye, target.Position, 0f);
            if (!_navigator.TryGoTo(target.Position))
            {
                Finish(SkillStatus.Failed);
                return;
            }

            _navigator.Steer();
        }

        private void Finish(SkillStatus status)
        {
            _navigator.Stop();
            Status = status;
        }
    }
}

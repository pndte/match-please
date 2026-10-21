using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Bots.Navigation;

namespace Bw.UseCases.Bots.Skills
{
    public sealed class ApproachSkill : IBotSkill
    {
        public SkillStatus Status { get; private set; }

        private readonly BotNavigator _navigator;

        public ApproachSkill(BotNavigator navigator)
        {
            _navigator = navigator;
        }

        public void Begin()
        {
            Status = SkillStatus.Running;
            _navigator.Stop();
        }

        public void Perform(Perception perception)
        {
            if (!perception.TryFindTarget(out var target) || _navigator.Status == NavStatus.Arrived)
            {
                Finish(SkillStatus.Failed);
                return;
            }

            if (target.Visible)
            {
                Finish(SkillStatus.Succeeded);
                return;
            }

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

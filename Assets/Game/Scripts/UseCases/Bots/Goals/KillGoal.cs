using Bw.UseCases.Bots.Goap;

namespace Bw.UseCases.Bots.Goals
{
    public sealed class KillGoal : IGoapGoal
    {
        private const float Urgent = 1f;

        public BotFact Wanted => BotFact.TargetDown;

        public float PriorityIn(BotFact facts) =>
            (facts & BotFact.TargetNoticed) != 0 ? Urgent : 0f;
    }
}

using Bw.UseCases.Bots.Goap;

namespace Bw.UseCases.Bots.Goals
{
    public sealed class KillGoal : IGoapGoal
    {
        private const float Urgent = 1f;
        private const BotFact Needs = BotFact.TargetNoticed;

        public BotFact Wanted => BotFact.TargetDown;

        private readonly float _weight;

        public KillGoal(float weight)
        {
            _weight = weight;
        }

        public float PriorityIn(BotAssessment assessment) =>
            (assessment.Facts & Needs) == Needs ? Urgent * _weight : 0f;
    }
}

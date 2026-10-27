using Bw.UseCases.Bots.Goap;

namespace Bw.UseCases.Bots.Goals
{
    public sealed class FinishOffGoal : IGoapGoal
    {
        private const float Urgent = 1.4f;
        private const BotFact Needs = BotFact.TargetNoticed | BotFact.TargetWounded;

        public BotFact Wanted => BotFact.TargetDown;

        private readonly float _weight;

        public FinishOffGoal(float weight)
        {
            _weight = weight;
        }

        public float PriorityIn(BotAssessment assessment) =>
            (assessment.Facts & Needs) == Needs ? Urgent * _weight : 0f;
    }
}

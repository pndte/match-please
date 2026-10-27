using Bw.UseCases.Bots.Goap;

namespace Bw.UseCases.Bots.Goals
{
    public sealed class RetreatGoal : IGoapGoal
    {
        private const float Urgent = 1.2f;
        private const BotFact Needs = BotFact.TargetNoticed | BotFact.Hurt;

        public BotFact Wanted => BotFact.Retreated;

        private readonly float _weight;

        public RetreatGoal(float weight)
        {
            _weight = weight;
        }

        public float PriorityIn(BotAssessment assessment) =>
            (assessment.Facts & Needs) == Needs ? (Urgent + 1f - assessment.Health) * _weight : 0f;
    }
}

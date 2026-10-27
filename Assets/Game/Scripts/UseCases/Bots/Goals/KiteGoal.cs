using Bw.UseCases.Bots.Goap;

namespace Bw.UseCases.Bots.Goals
{
    public sealed class KiteGoal : IGoapGoal
    {
        private const float Urgent = 1f;
        private const BotFact Needs = BotFact.TargetNoticed | BotFact.TargetInSight;

        public BotFact Wanted => BotFact.Kited;

        private readonly float _weight;

        public KiteGoal(float weight)
        {
            _weight = weight;
        }

        public float PriorityIn(BotAssessment assessment) =>
            (assessment.Facts & Needs) == Needs ? Urgent * _weight : 0f;
    }
}

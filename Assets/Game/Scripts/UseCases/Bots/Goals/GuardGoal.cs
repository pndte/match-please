using Bw.UseCases.Bots.Goap;

namespace Bw.UseCases.Bots.Goals
{
    public sealed class GuardGoal : IGoapGoal
    {
        private const float Urgent = 1.8f;
        private const BotFact Needs = BotFact.Threatened | BotFact.GuardReady;

        public BotFact Wanted => BotFact.Parried;

        private readonly float _weight;

        public GuardGoal(float weight)
        {
            _weight = weight;
        }

        public float PriorityIn(BotAssessment assessment) =>
            (assessment.Facts & Needs) == Needs ? Urgent * _weight : 0f;
    }
}

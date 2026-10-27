using Bw.UseCases.Bots.Goap;

namespace Bw.UseCases.Bots.Goals
{
    public sealed class PunishReloadGoal : IGoapGoal
    {
        private const float Urgent = 1.6f;
        private const BotFact Needs = BotFact.TargetNoticed | BotFact.TargetReloading;

        public BotFact Wanted => BotFact.TargetDown;

        private readonly float _weight;

        public PunishReloadGoal(float weight)
        {
            _weight = weight;
        }

        public float PriorityIn(BotAssessment assessment) =>
            (assessment.Facts & Needs) == Needs ? Urgent * _weight : 0f;
    }
}

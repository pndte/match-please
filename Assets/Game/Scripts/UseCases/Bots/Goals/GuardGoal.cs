using Bw.UseCases.Bots.Goap;

namespace Bw.UseCases.Bots.Goals
{
    public sealed class GuardGoal : IGoapGoal //TODO: раненый бот с ящиком не отступает: он подбегает к цели и бросает в неё ящик
    {
        private const float Urgent = 1.5f;
        private const BotFact Danger = BotFact.Hurt | BotFact.TargetNoticed | BotFact.PropNearby;

        public BotFact Wanted => BotFact.PropHeld;

        public float PriorityIn(BotFact facts) =>
            (facts & Danger) == Danger ? Urgent : 0f;
    }
}

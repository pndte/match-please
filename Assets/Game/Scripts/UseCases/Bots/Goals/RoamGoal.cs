using Bw.UseCases.Bots.Goap;

namespace Bw.UseCases.Bots.Goals
{
    public sealed class RoamGoal : IGoapGoal
    {
        private const float Idle = 0.1f;

        public BotFact Wanted => BotFact.Roamed;

        public float PriorityIn(BotFact facts) =>
            Idle;
    }
}

namespace Bw.UseCases.Bots.Goap
{
    public readonly struct BotAssessment
    {
        public readonly BotFact Facts;
        public readonly float Health;

        public BotAssessment(BotFact facts, float health)
        {
            Facts = facts;
            Health = health;
        }
    }
}

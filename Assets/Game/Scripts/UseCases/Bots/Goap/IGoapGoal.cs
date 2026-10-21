namespace Bw.UseCases.Bots.Goap
{
    public interface IGoapGoal
    {
        public BotFact Wanted { get; }

        public float PriorityIn(BotFact facts);
    }
}

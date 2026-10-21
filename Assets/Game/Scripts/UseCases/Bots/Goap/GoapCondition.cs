namespace Bw.UseCases.Bots.Goap
{
    public readonly struct GoapCondition
    {
        public readonly BotFact Needs;
        public readonly BotFact Forbids;

        public GoapCondition(BotFact needs, BotFact forbids)
        {
            Needs = needs;
            Forbids = forbids;
        }
    }
}

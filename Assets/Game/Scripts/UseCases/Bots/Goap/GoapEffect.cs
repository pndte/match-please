namespace Bw.UseCases.Bots.Goap
{
    public readonly struct GoapEffect
    {
        public readonly BotFact Gives;
        public readonly BotFact Takes;

        public GoapEffect(BotFact gives, BotFact takes)
        {
            Gives = gives;
            Takes = takes;
        }
    }
}

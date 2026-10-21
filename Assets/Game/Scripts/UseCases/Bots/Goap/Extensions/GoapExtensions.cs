namespace Bw.UseCases.Bots.Goap.Extensions
{
    public static class GoapExtensions
    {
        public static bool IsMetBy(this GoapCondition condition, BotFact facts) =>
            (facts & condition.Needs) == condition.Needs && (facts & condition.Forbids) == 0;

        public static BotFact AppliedTo(this GoapEffect effect, BotFact facts) =>
            (facts | effect.Gives) & ~effect.Takes;

        public static bool IsMetBy(this IGoapGoal goal, BotFact facts) =>
            (facts & goal.Wanted) == goal.Wanted;
    }
}

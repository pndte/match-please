using Bw.UseCases.Bots.Abstractions;

namespace Bw.UseCases.Bots.Goap
{
    public sealed class GoapAction
    {
        public GoapCondition When { get; }
        public GoapEffect Then { get; }
        public float Cost { get; }
        public IBotSkill Skill { get; }

        public GoapAction(
            GoapCondition when,
            GoapEffect then,
            float cost,
            IBotSkill skill)
        {
            When = when;
            Then = then;
            Cost = cost;
            Skill = skill;
        }
    }
}

namespace Bw.UseCases.Bots.Abstractions
{
    public interface IBotSkill
    {
        public SkillStatus Status { get; }

        public void Begin();
        public void Perform(Perception perception);
    }
}

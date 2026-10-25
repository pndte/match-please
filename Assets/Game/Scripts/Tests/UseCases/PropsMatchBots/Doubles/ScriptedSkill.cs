using System.Collections.Generic;
using Bw.UseCases.Bots.Abstractions;

namespace Bw.Tests.UseCases.PropsMatchBots.Doubles
{
    public sealed class ScriptedSkill : IBotSkill
    {
        public SkillStatus Status { get; private set; }
        public string Name { get; }
        public int Begun { get; private set; }
        public IReadOnlyList<Perception> Performed => _performed;
        public SkillStatus Outcome { get; set; } = SkillStatus.Running;

        private readonly List<Perception> _performed = new();

        public ScriptedSkill(string name)
        {
            Name = name;
        }

        public void Begin()
        {
            Begun++;
            Status = SkillStatus.Running;
        }

        public void Perform(Perception perception)
        {
            _performed.Add(perception);
            Status = Outcome;
        }

        public override string ToString() =>
            Name;
    }
}

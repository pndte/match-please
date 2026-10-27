using System;
using Bw.UseCases.Bots.Abstractions;

namespace Bw.UseCases.Bots.Skills
{
    public sealed class ShootSkill : IBotSkill
    {
        public SkillStatus Status { get; private set; }

        private readonly BotGunner _gunner;

        public ShootSkill(BotGunner gunner)
        {
            _gunner = gunner;
        }

        public void Begin()
        {
            Status = SkillStatus.Running;
            _gunner.Begin();
        }

        public void Perform(Perception perception)
        {
            if (!perception.TryFindTarget(out var target))
                throw new InvalidOperationException("The shoot skill aims at a target in sight: the brain runs it only while the bot sees one.");

            _gunner.FireAt(perception.Eye, target);
        }
    }
}

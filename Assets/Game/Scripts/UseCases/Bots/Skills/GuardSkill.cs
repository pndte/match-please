using System;
using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Bots.Extensions;

namespace Bw.UseCases.Bots.Skills
{
    public sealed class GuardSkill : IBotSkill
    {
        public SkillStatus Status { get; private set; }

        private readonly IBotGamepad _gamepad;

        public GuardSkill(IBotGamepad gamepad)
        {
            _gamepad = gamepad;
        }

        public void Begin() =>
            Status = SkillStatus.Running;

        public void Perform(Perception perception)
        {
            if (!perception.Threat.Noticed)
                throw new InvalidOperationException("The guard skill guards against a noticed attack: the brain runs it only while the bot is threatened.");

            _gamepad.RaiseGuard(perception.Eye, perception.Threat.Attacker.Movement.State.Value.Body.Position);
        }
    }
}

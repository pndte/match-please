using System;
using Bw.Entities.Simulation;
using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Bots.Extensions;
using Bw.UseCases.Bots.Goap;
using Bw.UseCases.Movement;
using UnityEngine;

namespace Bw.UseCases.Bots.Skills
{
    public sealed class ChargedPunchSkill : IBotSkill //TODO: с зарядом бот идёт к цели напрямую, а не по графу: так он догоняет её только на одной платформе
    {
        private const float GiveUpTime = 1f;

        public SkillStatus Status { get; private set; }

        private readonly IBotGamepad _gamepad;
        private readonly BotDifficultyConfig _difficulty;
        private readonly ISimulationStep _step;

        private int _ticks;

        public ChargedPunchSkill(IBotGamepad gamepad, BotDifficultyConfig difficulty, ISimulationStep step)
        {
            _gamepad = gamepad;
            _difficulty = difficulty;
            _step = step;
        }

        public void Begin()
        {
            Status = SkillStatus.Running;
            _ticks = 0;
        }

        public void Perform(Perception perception)
        {
            if (!perception.TryFindTarget(out var target))
                throw new InvalidOperationException("The charged punch skill charges at a close target: the brain runs it only while the bot sees one.");

            var inReach = (perception.Facts & BotFact.TargetInReach) != 0;
            var charged = _ticks >= Ticks(_difficulty.ChargeTime);
            if (charged && inReach || _ticks >= Ticks(_difficulty.ChargeTime + GiveUpTime))
            {
                Status = SkillStatus.Succeeded;
                return;
            }

            _ticks++;
            _gamepad.AimAt(perception.Eye, target.Position, 0f);
            _gamepad.PullTrigger();
            if (!inReach)
                _gamepad.Steer(new MovementInput(Mathf.Sign(target.Position.x - perception.Eye.x), false, false, false));
        }

        private int Ticks(float seconds) =>
            Mathf.CeilToInt(seconds / _step.Duration);
    }
}

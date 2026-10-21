using System;
using Bw.Entities.Simulation;
using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Bots.Extensions;
using Bw.UseCases.Bots.Goap;
using UnityEngine;

namespace Bw.UseCases.Bots.Skills
{
    public sealed class PunchSkill : IBotSkill
    {
        private const int AwayTicks = 10;

        public SkillStatus Status { get; private set; }

        private readonly IBotGamepad _gamepad;
        private readonly BotsConfig _bots;
        private readonly ISimulationStep _step;

        private int _ticks;
        private int _away;

        public PunchSkill(IBotGamepad gamepad, BotsConfig bots, ISimulationStep step)
        {
            _gamepad = gamepad;
            _bots = bots;
            _step = step;
        }

        public void Begin()
        {
            Status = SkillStatus.Running;
            _ticks = 0;
            _away = 0;
        }

        public void Perform(Perception perception)
        {
            if (!perception.TryFindTarget(out var target))
                throw new InvalidOperationException("The punch skill jabs at a target in reach: the brain runs it only while the bot sees one.");

            _gamepad.AimAt(perception.Eye, target.Position, 0f);
            if ((perception.Facts & BotFact.TargetInReach) == 0)
            {
                if (++_away > AwayTicks)
                    Status = SkillStatus.Failed;

                return;
            }

            _away = 0;
            if (_ticks++ % JabTicks() == 0)
                _gamepad.PullTrigger();
        }

        private int JabTicks() =>
            Mathf.CeilToInt(_bots.Fists.SwingTime / _step.Duration) + 1;
    }
}

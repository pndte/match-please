using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Bots.Extensions;
using Bw.UseCases.Combat.Abstractions.Weapons;
using UnityEngine;

namespace Bw.UseCases.Bots.Skills
{
    public sealed class ThrowSkill : IBotSkill //TODO: подъём прицела над целью подобран на глаз, а не из скорости броска и гравитации ящика
    {
        private const int PressTick = 1;
        private const int GiveUpTicks = 30;
        private const float LiftPerUnit = 1.5f;
        private const float MaxLift = 12f;

        public SkillStatus Status { get; private set; }

        private readonly BotInventory _inventory;
        private readonly IBotGamepad _gamepad;

        private int _ticks;

        public ThrowSkill(BotInventory inventory, IBotGamepad gamepad)
        {
            _inventory = inventory;
            _gamepad = gamepad;
        }

        public void Begin()
        {
            Status = SkillStatus.Running;
            _ticks = 0;
        }

        public void Perform(Perception perception)
        {
            if (!_inventory.IsDrawn(WeaponKind.Prop))
            {
                Status = SkillStatus.Succeeded;
                return;
            }

            if (!perception.TryFindTarget(out var target) || _ticks > GiveUpTicks)
            {
                Status = SkillStatus.Failed;
                return;
            }

            var lift = Mathf.Min(MaxLift, (target.Position - perception.Eye).magnitude * LiftPerUnit);
            var leftward = target.Position.x < perception.Eye.x;
            _gamepad.AimAt(perception.Eye, target.Position, leftward ? -lift : lift);
            if (_ticks++ == PressTick)
                _gamepad.PullTrigger();
        }
    }
}

using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Bots.Skills
{
    public sealed class DrawSkill : IBotSkill
    {
        private const int WaitTicks = 45;

        public SkillStatus Status { get; private set; }

        private readonly BotInventory _inventory;
        private readonly WeaponKind _kind;

        private int _ticks;

        public DrawSkill(BotInventory inventory, WeaponKind kind)
        {
            _inventory = inventory;
            _kind = kind;
        }

        public void Begin()
        {
            Status = SkillStatus.Running;
            _ticks = 0;
        }

        public void Perform(Perception perception)
        {
            if (_inventory.IsDrawn(_kind))
            {
                Status = SkillStatus.Succeeded;
                return;
            }

            if (_inventory.TryFindDrawable(_kind, out var weapon))
            {
                _inventory.Draw(weapon);
                Status = SkillStatus.Succeeded;
                return;
            }

            if (++_ticks > WaitTicks)
                Status = SkillStatus.Failed;
        }
    }
}

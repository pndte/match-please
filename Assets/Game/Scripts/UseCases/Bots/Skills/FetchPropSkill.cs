using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Bots.Navigation;
using Bw.UseCases.Bots.Senses;
using Bw.UseCases.Combat.Abstractions.Weapons;

namespace Bw.UseCases.Bots.Skills
{
    public sealed class FetchPropSkill : IBotSkill //TODO: бот идёт к ближайшему ящику, даже когда тот лежит у цели: отступать с ним он может начать навстречу ей
    {
        public SkillStatus Status { get; private set; }

        private readonly BotProps _props;
        private readonly BotInventory _inventory;
        private readonly BotNavigator _navigator;

        public FetchPropSkill(BotProps props, BotInventory inventory, BotNavigator navigator)
        {
            _props = props;
            _inventory = inventory;
            _navigator = navigator;
        }

        public void Begin()
        {
            Status = SkillStatus.Running;
            _navigator.Stop();
        }

        public void Perform(Perception perception)
        {
            if (_inventory.IsDrawn(WeaponKind.Prop))
            {
                Finish(SkillStatus.Succeeded);
                return;
            }

            if (!_props.TryFindNearest(out var prop) || _navigator.Status == NavStatus.Arrived && !_props.IsPickable(prop))
            {
                Finish(SkillStatus.Failed);
                return;
            }

            if (_props.IsPickable(prop) && _inventory.TryPickUp(prop.Prop))
                return;

            if (!_navigator.TryGoTo(prop.Position))
            {
                Finish(SkillStatus.Failed);
                return;
            }

            _navigator.Steer();
        }

        private void Finish(SkillStatus status)
        {
            _navigator.Stop();
            Status = status;
        }
    }
}

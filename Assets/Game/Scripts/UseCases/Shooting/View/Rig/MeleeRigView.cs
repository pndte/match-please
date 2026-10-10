using Bw.Entities.Extensions;
using Bw.Entities.Simulation;
using Bw.UseCases.Shooting.Melee;
using Bw.UseCases.Shooting.Melee.Extensions;
using Bw.UseCases.Shooting.Weapon;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Rig
{
    public sealed class MeleeRigView : IStateView<WeaponState> //TODO: тело персонажа не встаёт в позу удара (состояние Punch в Sanya.controller и его overrides): кулакам нужен доступ к аниматору того, кто их держит
    {
        private static readonly int HeldId = Animator.StringToHash(WeaponRigParameters.Held);
        private static readonly int PunchId = Animator.StringToHash(WeaponRigParameters.Punch);
        private static readonly int PunchVariantId = Animator.StringToHash(WeaponRigParameters.PunchVariant);

        private readonly Animator _rig;
        private readonly ISimulationStep _step;
        private readonly MeleeWeaponConfig _config;

        private bool _synchronized;
        private int _swings;

        public MeleeRigView(
            Lifetime lifetime,
            Animator rig,
            IReadonlyWeaponDraw draw,
            ISimulationStep step,
            MeleeWeaponConfig config)
        {
            _rig = rig;
            _step = step;
            _config = config;

            draw.DrawnLifetime.WhenAlive(lifetime, drawnLifetime =>
            {
                rig.SetBool(HeldId, true);
                drawnLifetime.OnTermination(() => rig.SetBool(HeldId, false));
            });
        }

        public void Show(WeaponState from, WeaponState to, float progress)
        {
            var swings = Swings(to);
            if (_synchronized && swings > _swings)
            {
                _rig.SetInteger(PunchVariantId, swings % WeaponRigParameters.PunchVariants);
                _rig.SetTrigger(PunchId);
            }

            _swings = swings;
            _synchronized = true;
        }

        private int Swings(WeaponState state) =>
            state.CooldownTicks > _config.SwingTicks(_step) - _config.StrikeTicks(_step) ? state.Shots + 1 : state.Shots;
    }
}

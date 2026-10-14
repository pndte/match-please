using Bw.UseCases.Shooting.View.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Rig
{
    public sealed class MeleeRigView
    {
        private static readonly int HeldId = Animator.StringToHash(MeleeRigParameters.Held);
        private static readonly int PunchId = Animator.StringToHash(MeleeRigParameters.Punch);
        private static readonly int PunchHandId = Animator.StringToHash(MeleeRigParameters.PunchHand);
        private static readonly int PunchVariantId = Animator.StringToHash(MeleeRigParameters.PunchVariant);
        private static readonly int ChargingId = Animator.StringToHash(MeleeRigParameters.Charging);
        private static readonly int StrongPunchId = Animator.StringToHash(MeleeRigParameters.StrongPunch);
        private static readonly int StrongVariantId = Animator.StringToHash(MeleeRigParameters.StrongVariant);

        private readonly Animator _rig;

        public MeleeRigView(
            Lifetime lifetime,
            Animator rig,
            IWeaponCarryViewModel carry,
            IMeleeSwingsViewModel swings)
        {
            _rig = rig;

            carry.Drawn.WhenTrue(lifetime, drawnLifetime =>
            {
                rig.SetBool(HeldId, true);
                drawnLifetime.OnTermination(() => rig.SetBool(HeldId, false));
            });
            swings.Started.Advise(lifetime, Punch);
            swings.Charging.WhenTrue(lifetime, chargingLifetime =>
            {
                rig.SetBool(ChargingId, true);
                chargingLifetime.OnTermination(() => rig.SetBool(ChargingId, false));
            });
        }

        private void Punch(MeleeSwing swing)
        {
            if (swing.Strong)
                StrongPunch(swing.Number - 1);
            else
                Jab(swing.Number - 1);
        }

        private void Jab(int blow)
        {
            _rig.SetInteger(PunchHandId, blow % MeleeRigParameters.Hands);
            _rig.SetInteger(PunchVariantId, blow / MeleeRigParameters.Hands % MeleeRigParameters.VariantsPerHand);
            _rig.SetTrigger(PunchId);
        }

        private void StrongPunch(int blow)
        {
            _rig.SetInteger(StrongVariantId, blow % MeleeRigParameters.StrongVariants);
            _rig.SetTrigger(StrongPunchId);
        }
    }
}

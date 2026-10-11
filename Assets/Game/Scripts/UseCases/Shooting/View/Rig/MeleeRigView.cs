using Bw.Entities.Extensions;
using Bw.UseCases.Shooting.View.Abstractions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
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

        private readonly Animator _rig;

        public MeleeRigView(Lifetime lifetime, Animator rig, IReadonlyWeaponDraw draw, IMeleeSwings swings)
        {
            _rig = rig;

            draw.DrawnLifetime.WhenAlive(lifetime, drawnLifetime =>
            {
                rig.SetBool(HeldId, true);
                drawnLifetime.OnTermination(() => rig.SetBool(HeldId, false));
            });
            swings.Started.Advise(lifetime, Punch);
        }

        private void Punch(int swing)
        {
            var blow = swing - 1;
            _rig.SetInteger(PunchHandId, blow % MeleeRigParameters.Hands);
            _rig.SetInteger(PunchVariantId, blow / MeleeRigParameters.Hands % MeleeRigParameters.VariantsPerHand);
            _rig.SetTrigger(PunchId);
        }
    }
}

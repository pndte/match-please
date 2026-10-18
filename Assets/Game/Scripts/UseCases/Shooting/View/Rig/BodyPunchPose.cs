using Bw.Entities.Extensions;
using Bw.UseCases.Character.View.Abstractions;
using Bw.UseCases.Shooting.View.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.View.Rig
{
    public sealed class BodyPunchPose
    {
        public BodyPunchPose(Lifetime lifetime, Transform weapon, IMeleeSwingsViewModel swings)
        {
            swings.Started.Advise(lifetime, _ => weapon.HolderService<ICharacterPoses>().Punch());
        }
    }
}

using Bw.Entities.Extensions;
using Bw.UseCases.Combat.View.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.View.Rig
{
    public sealed class BodyPunchPose
    {
        public BodyPunchPose(Lifetime lifetime, IWeaponCarryViewModel carry, IMeleeSwingsViewModel swings)
        {
            swings.Started.Advise(lifetime, _ => carry.Holder.Poses.Punch());
        }
    }
}

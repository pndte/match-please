using Bw.Entities.Extensions;
using Bw.UseCases.Combat.View.Abstractions;
using Bw.UseCases.Props.View.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Props.View.Throw
{
    public sealed class ThrowBodyPose
    {
        public ThrowBodyPose(Lifetime lifetime, IWeaponCarryViewModel carry, IPropThrowsViewModel throws)
        {
            throws.WindUpStarted.Advise(lifetime, _ => carry.Holder.Poses.Throw());
        }
    }
}

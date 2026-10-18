using Bw.Entities.Extensions;
using Bw.UseCases.Character.View.Abstractions;
using Bw.UseCases.Props.View.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Props.View.Throw
{
    public sealed class ThrowBodyPose
    {
        public ThrowBodyPose(Lifetime lifetime, Transform prop, IPropThrowsViewModel throws)
        {
            throws.WindUpStarted.Advise(lifetime, _ => prop.HolderService<ICharacterPoses>().Throw());
        }
    }
}

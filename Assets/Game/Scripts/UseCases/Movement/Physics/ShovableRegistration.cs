using Bw.Entities.Extensions;
using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class ShovableRegistration
    {
        public ShovableRegistration(
            Lifetime lifetime,
            PhysicsBody body,
            IShovable shovable,
            IShovableCollection shovables)
        {
            shovables.AddLifetimed(lifetime, body, shovable);
        }
    }
}

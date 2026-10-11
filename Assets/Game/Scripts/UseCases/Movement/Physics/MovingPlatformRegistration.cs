using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class MovingPlatformRegistration
    {
        public MovingPlatformRegistration(Lifetime lifetime, IMovingPlatform platform, IMovingPlatformCollection platforms)
        {
            platforms.AddLifetimed(lifetime, platform);
        }
    }
}

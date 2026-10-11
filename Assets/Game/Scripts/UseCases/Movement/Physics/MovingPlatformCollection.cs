using Bw.UseCases.Movement.Physics.Abstractions;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Movement.Physics
{
    public sealed class MovingPlatformCollection : ViewableList<IMovingPlatform>, IMovingPlatformCollection
    {
    }
}

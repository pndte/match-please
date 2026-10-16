using JetBrains.Collections.Viewable;
using JetBrains.Core;

namespace Bw.Entities.Physics
{
    public interface IPhysicsSteps
    {
        public ISource<Unit> Stepped { get; }
    }
}

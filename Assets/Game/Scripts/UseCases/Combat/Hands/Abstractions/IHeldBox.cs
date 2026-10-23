using Bw.Entities.Physics;
using Bw.UseCases.Combat.Abstractions.Hands;
using JetBrains.Collections.Viewable;
using JetBrains.Core;

namespace Bw.UseCases.Combat.Hands.Abstractions
{
    public interface IHeldBox
    {
        public BodyBox Box { get; }
        public int Layer { get; }
        public ISource<Unit> Turned { get; }

        public GripPose PoseIn(HoldPose hands, GripPose previous);
    }
}

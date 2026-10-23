using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Movement;
using Bw.UseCases.Props.Extensions;

namespace Bw.UseCases.Props.Network
{
    public sealed class ThrowLaunch
    {
        private readonly ReleaseSpots _spots;
        private readonly HeldPropPose _pose;
        private readonly PropThrowConfig _config;

        public ThrowLaunch(ReleaseSpots spots, HeldPropPose pose, PropThrowConfig config)
        {
            _spots = spots;
            _pose = pose;
            _config = config;
        }

        public RigidState Dropped(IReadonlyCharacter holder) =>
            _spots.Nearest(holder, holder.Hands.Grip);

        public RigidState Thrown(IReadonlyCharacter holder, float aim) =>
            _config.Thrown(_spots.Nearest(holder, _pose.PoseAt(holder.Hands.Pose, aim)), aim, holder.Movement.State.Value.Body.Velocity);
    }
}

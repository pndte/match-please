using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Weapons;

namespace Bw.UseCases.Combat.Abstractions.Hands
{
    public interface IReadonlyHands : IReadonlySimulation<HandsState>
    {
        public bool Busy { get; }
        public HoldPose Pose { get; }
        public GripPose Grip { get; }

        public bool TryFindInHands(out IWeapon weapon);
    }

    public interface IHands : IReadonlyHands
    {
        public bool FreeFor(IWeapon item);
        public void Take(IWeapon item);
    }
}

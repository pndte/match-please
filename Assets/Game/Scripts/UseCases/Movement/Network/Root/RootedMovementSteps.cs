using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;

namespace Bw.UseCases.Movement.Network.Root
{
    public sealed class RootedMovementSteps : ISteppable<MovementInput>
    {
        private readonly ISteppable<MovementInput> _movement;
        private readonly IReadonlyMovementRoot _root;
        private readonly INetworkTicks _ticks;

        public RootedMovementSteps(ISteppable<MovementInput> movement, IReadonlyMovementRoot root, INetworkTicks ticks)
        {
            _movement = movement;
            _root = root;
            _ticks = ticks;
        }

        public void Step(MovementInput input) =>
            _movement.Step(_root.RootedAt(_ticks.Current) ? default : input);
    }
}

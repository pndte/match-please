using Bw.Entities.Simulation;

namespace Bw.UseCases.Movement.Abstractions
{
    public interface IReadonlyMovement : IReadonlySimulation<MovementState>
    {
    }
}

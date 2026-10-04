using Bw.Entities.Simulation;

namespace Bw.UseCases.Movement.Physics.Abstractions
{
    public interface IMovementMotor : ISimulator<MovementInput, MovementState>
    {
    }
}

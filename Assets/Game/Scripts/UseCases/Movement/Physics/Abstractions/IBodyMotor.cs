namespace Bw.UseCases.Movement.Physics.Abstractions
{
    public interface IBodyMotor
    {
        public BodyState Step(BodyState state, BodyDrive drive, out bool launched);
    }
}

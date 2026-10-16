namespace Bw.UseCases.Movement.Network.Prediction
{
    public interface IPredictedRigidBody
    {
        public bool TryGetShown(double tick, out RigidState shown);
        public void Follow(double tick, RigidState shown);
    }
}

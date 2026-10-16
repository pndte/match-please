using JetBrains.Lifetimes;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public interface IPhysicsRollback
    {
        public void Track(Lifetime lifetime, IRollbackBody body);
        public void Request(int tick);
    }
}

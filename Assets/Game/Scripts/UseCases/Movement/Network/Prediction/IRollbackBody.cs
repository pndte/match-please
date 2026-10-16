using UnityEngine.LowLevelPhysics2D;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public interface IRollbackBody
    {
        public PhysicsBody Body { get; }
        public void Restore(int tick);
        public void Prepare(int tick);
        public void Finish(int tick);
        public void PredictTouched(int tick, IPredictableCollection predictables);
        public void Verify(int tick);
    }
}

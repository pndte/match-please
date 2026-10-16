using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Movement.Network.Prediction
{
    public interface IRigidStateEffects
    {
        public ISource<int> Changed { get; }
        public bool TryGetAffected(int tick, RigidState state, out RigidState affected);
    }
}

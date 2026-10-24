using JetBrains.Lifetimes;

namespace Bw.Entities.Network.LagCompensation
{
    public interface IRewindable //TODO: понять нужно ли это в целом
    {
        public void Rewind(Lifetime lifetime, ShooterView view);
    }
}

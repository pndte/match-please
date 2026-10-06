using JetBrains.Lifetimes;

namespace Bw.Entities.Network.LagCompensation
{
    public interface IRewindable
    {
        public void Rewind(Lifetime lifetime, double tick);
    }
}

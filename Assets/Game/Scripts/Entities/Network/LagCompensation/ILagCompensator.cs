using JetBrains.Lifetimes;

namespace Bw.Entities.Network.LagCompensation
{
    public interface ILagCompensator
    {
        public void Rewind(Lifetime lifetime, double tick);
    }
}

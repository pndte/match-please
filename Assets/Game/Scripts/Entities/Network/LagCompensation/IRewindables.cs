using JetBrains.Lifetimes;

namespace Bw.Entities.Network.LagCompensation
{
    public interface IRewindables
    {
        public void Track(Lifetime lifetime, IRewindable rewindable);
    }
}

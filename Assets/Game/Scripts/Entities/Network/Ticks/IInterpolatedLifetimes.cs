using JetBrains.Lifetimes;

namespace Bw.Entities.Network.Ticks
{
    public interface IInterpolatedLifetimes
    {
        public Lifetime CreateUntilShown(Lifetime owner);
    }
}

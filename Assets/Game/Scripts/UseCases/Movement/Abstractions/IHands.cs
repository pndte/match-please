using JetBrains.Lifetimes;

namespace Bw.UseCases.Movement.Abstractions
{
    public interface IHands
    {
        public void Take(Lifetime lifetime, IHeldItem item);
    }
}

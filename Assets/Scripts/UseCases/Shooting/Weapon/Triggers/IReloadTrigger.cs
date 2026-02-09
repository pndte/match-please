using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Triggers
{
    public interface IReloadTrigger
    {
        public ISignal<Lifetime> Triggered { get; }
    }
}
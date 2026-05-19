using Cysharp.Threading.Tasks;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IReloader
    {
        public IReadonlyProperty<bool> CanReload { get; }
        public UniTaskVoid Reload(Lifetime lifetime);
        public IReadonlyProperty<ReloadState> State { get; }
    }

    public enum ReloadState
    {
        Reloading,
        Interrupted,
        Complete
    }
}
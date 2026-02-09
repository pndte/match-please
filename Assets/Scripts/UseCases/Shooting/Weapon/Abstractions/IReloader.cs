using Cysharp.Threading.Tasks;
using JetBrains.Collections.Viewable;
using JetBrains.Core;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IReloader
    {
        public UniTaskVoid Reload(Lifetime lifetime);
        public IReadonlyProperty<bool> Reloading { get; } //TODO: вероятно стоит заменить две переменные на одну, но передавать не буль, а структуру данных с состоянием перезарядки
        public ISource<Unit> ReloadComplete { get; }
    }
}
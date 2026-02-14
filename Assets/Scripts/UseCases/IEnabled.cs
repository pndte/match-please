using Bw.Entities;
using Bw.UseCases.Players;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases
{
    public interface IControlledBy : IReadonlyControlledBy
    {
        public void Set(Lifetime lifetime, IPlayer player);
    }
    
    public interface IReadonlyControlledBy
    {
        public IReadonlyProperty<bool> Me { get; }
        public IReadonlyViewableList<IPlayer> Users { get; } //TODO: перенести в верхний интерфейс, сейчас не даёт сделать это WeaponShotTriggerSynchronizer
    }
}
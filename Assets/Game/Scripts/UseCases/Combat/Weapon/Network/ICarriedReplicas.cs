using Bw.UseCases.Combat.Abstractions.Characters;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public interface ICarriedReplicas //TODO: реплика инвентаря — сетевая часть персонажа, её ищут по персонажу; бросок с опережением уходит вместе со старым предсказанием в новом сетевом ядре
    {
        public IViewableMap<IReadonlyCharacter, ICarriedReplica> ByCharacter { get; }
    }
}

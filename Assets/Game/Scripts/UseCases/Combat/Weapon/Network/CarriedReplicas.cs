using Bw.UseCases.Combat.Abstractions.Characters;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class CarriedReplicas : ICarriedReplicas
    {
        public IViewableMap<IReadonlyCharacter, ICarriedReplica> ByCharacter { get; } = new ViewableMap<IReadonlyCharacter, ICarriedReplica>();
    }
}

using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Characters;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class CarriedReplicaRegistration
    {
        public CarriedReplicaRegistration(
            Lifetime lifetime,
            IReadonlyCharacter character,
            ICarriedReplica replica,
            ICarriedReplicas replicas)
        {
            replicas.ByCharacter.AddLifetimed(lifetime, character, replica);
        }
    }
}

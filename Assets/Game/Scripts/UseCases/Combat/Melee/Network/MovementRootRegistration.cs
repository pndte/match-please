using Bw.Entities.Extensions;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Movement.Network.Root;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Melee.Network
{
    public sealed class MovementRootRegistration
    {
        public MovementRootRegistration(
            Lifetime lifetime,
            IReadonlyCharacter character,
            IMovementRoot root,
            IMovementRoots roots)
        {
            roots.ByCharacter.AddLifetimed(lifetime, character, root);
        }
    }
}

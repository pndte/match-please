using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Movement.Network.Root;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Melee.Network
{
    public sealed class MovementRoots : IMovementRoots
    {
        public IViewableMap<IReadonlyCharacter, IMovementRoot> ByCharacter { get; } = new ViewableMap<IReadonlyCharacter, IMovementRoot>();
    }
}

using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Movement.Network.Root;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Melee.Network
{
    public interface IMovementRoots //TODO: корень движения — сетевая часть персонажа, поэтому его ищут по персонажу здесь, а не в ICharacter; новое сетевое ядро ставит корень в шаге персонажа
    {
        public IViewableMap<IReadonlyCharacter, IMovementRoot> ByCharacter { get; }
    }
}

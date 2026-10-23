using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public interface IReadonlyCharacterTrigger
    {
        public TriggerControls Current { get; }
    }

    public interface ICharacterTriggers
    {
        public IViewableMap<IReadonlyCharacter, IReadonlyCharacterTrigger> ByCharacter { get; }
    }
}

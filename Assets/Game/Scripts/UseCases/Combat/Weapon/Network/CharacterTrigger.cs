using Bw.UseCases.Combat.Weapon.Network.Requests;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class CharacterTrigger : IReadonlyCharacterTrigger
    {
        public TriggerControls Current { get; private set; }

        public void Take(TriggerControls controls) =>
            Current = controls;
    }
}

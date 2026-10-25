using Bw.UseCases.Character.Network.Requests;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Weapon.Network.Requests;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Movement.Network.Requests;

namespace Bw.Tests.UseCases.Character.Doubles
{
    public static class Controls
    {
        public static readonly TriggerPress Press = new(4, 3, 2.5f, 1.5f, 0);
        public static readonly MovementControls Running = new(1f, false, true, false);
        public static readonly HandsControls Firing = new(30f, new TriggerControls(true, 2.5f, 1.5f, Press), false, DrawControl.Keep);
        public static readonly CharacterControls RunningAndFiring = new(Running, Firing, GuardStance.Lowered);
    }
}

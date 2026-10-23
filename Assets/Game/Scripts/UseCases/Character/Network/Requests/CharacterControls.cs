using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Mortality.Guarding;
using Bw.UseCases.Movement.Network.Requests;

namespace Bw.UseCases.Character.Network.Requests
{
    public readonly struct CharacterControls
    {
        public readonly MovementControls Movement;
        public readonly HandsControls Hands;
        public readonly GuardStance Guard;

        public CharacterControls(MovementControls movement, HandsControls hands, GuardStance guard)
        {
            Movement = movement;
            Hands = hands;
            Guard = guard;
        }
    }
}

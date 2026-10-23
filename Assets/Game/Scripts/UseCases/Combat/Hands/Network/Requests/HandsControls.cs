using Bw.UseCases.Combat.Weapon.Network.Requests;

namespace Bw.UseCases.Combat.Hands.Network.Requests
{
    public readonly struct HandsControls
    {
        public readonly float Aim;
        public readonly TriggerControls Trigger;
        public readonly bool ReloadPressed;
        public readonly DrawControl Choice;

        public HandsControls(
            float aim,
            TriggerControls trigger,
            bool reloadPressed,
            DrawControl choice)
        {
            Aim = aim;
            Trigger = trigger;
            ReloadPressed = reloadPressed;
            Choice = choice;
        }
    }
}

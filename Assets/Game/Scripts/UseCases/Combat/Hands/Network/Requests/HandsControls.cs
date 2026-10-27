using Bw.UseCases.Combat.Weapon.Network.Requests;

using UnityEngine;

namespace Bw.UseCases.Combat.Hands.Network.Requests
{
    public readonly struct HandsControls
    {
        public readonly Vector2 AimPoint;
        public readonly TriggerControls Trigger;
        public readonly bool ReloadPressed;
        public readonly DrawControl Choice;

        public HandsControls(
            Vector2 aimPoint,
            TriggerControls trigger,
            bool reloadPressed,
            DrawControl choice)
        {
            AimPoint = aimPoint;
            Trigger = trigger;
            ReloadPressed = reloadPressed;
            Choice = choice;
        }
    }
}

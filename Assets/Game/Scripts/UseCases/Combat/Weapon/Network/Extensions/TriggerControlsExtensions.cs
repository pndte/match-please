using Bw.UseCases.Combat.Weapon.Network.Requests;

namespace Bw.UseCases.Combat.Weapon.Network.Extensions
{
    public static class TriggerControlsExtensions
    {
        public static TriggerControls WithoutFreshPress(this TriggerControls trigger) =>
            new(trigger.Pulled && trigger.Press.Age > 0, trigger.ViewDelay, trigger.PredictedDelay, trigger.Press);

        public static TriggerControls Pressed(this TriggerControls trigger, TriggerPress press, int lateTicks) =>
            new(true, press.ViewDelay + lateTicks, press.PredictedDelay + lateTicks, press.Delivered(lateTicks));

        public static TriggerControls WithPress(this TriggerControls trigger, TriggerPress press) =>
            new(trigger.Pulled, trigger.ViewDelay, trigger.PredictedDelay, press);

        public static bool IsFinite(this TriggerControls trigger) =>
            float.IsFinite(trigger.ViewDelay)
            && float.IsFinite(trigger.PredictedDelay)
            && float.IsFinite(trigger.Press.ViewDelay)
            && float.IsFinite(trigger.Press.PredictedDelay);
    }
}

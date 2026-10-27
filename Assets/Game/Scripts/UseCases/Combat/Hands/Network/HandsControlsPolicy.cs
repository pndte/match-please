using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Hands.Network.Requests;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Network.Extensions;
using Bw.UseCases.Combat.Weapon.Network.Requests;

namespace Bw.UseCases.Combat.Hands.Network
{
    public sealed class HandsControlsPolicy : IInputPolicy<HandsControls>
    {
        private readonly LagCompensationConfig _lagCompensation;
        private readonly ISimulationStep _step;
        private readonly WeaponInventoryConfig _config;

        public HandsControlsPolicy(LagCompensationConfig lagCompensation, ISimulationStep step, WeaponInventoryConfig config)
        {
            _lagCompensation = lagCompensation;
            _step = step;
            _config = config;
        }

        public bool IsValid(HandsControls controls) =>
            float.IsFinite(controls.AimPoint.x) && float.IsFinite(controls.AimPoint.y) && controls.Trigger.IsFinite() && IsValid(controls.Choice);

        public HandsControls CaughtUp(
            HandsControls controls,
            HandsControls lastControls,
            HandsControls heard,
            int heardTicksAgo) =>
            new(controls.AimPoint, CaughtUp(controls.Trigger, lastControls.Trigger, heard.Trigger, heardTicksAgo), controls.ReloadPressed, controls.Choice);

        private TriggerControls CaughtUp(
            TriggerControls trigger,
            TriggerControls lastTrigger,
            TriggerControls heard,
            int heardTicksAgo)
        {
            var press = heard.Press;
            var lateTicks = heardTicksAgo + press.Age;
            if (press.Number == lastTrigger.Press.Number || lateTicks < 0)
                return trigger.WithPress(lastTrigger.Press.WithAge(lastTrigger.Press.Age + 1));

            return lateTicks <= _lagCompensation.RewindTicks(_step)
                ? trigger.Pressed(press, lateTicks)
                : trigger.WithPress(press.WithAge(lateTicks));
        }

        private bool IsValid(DrawControl choice) =>
            choice.Kind switch
            {
                DrawControlKind.Keep => choice.Value == 0,
                DrawControlKind.Last => choice.Value == 0,
                DrawControlKind.Slot => choice.Value >= 0 && choice.Value < _config.Slots.Length,
                DrawControlKind.Scroll => choice.Value == 1 || choice.Value == -1,
                _ => false
            };
    }
}

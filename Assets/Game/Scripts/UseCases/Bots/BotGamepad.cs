using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Movement;

namespace Bw.UseCases.Bots
{
    public sealed class BotGamepad : IBotGamepad
    {
        public MovementInput Movement { get; private set; }
        public float Aim { get; private set; }
        public bool TriggerPulled { get; private set; }
        public bool ReloadPressed { get; private set; }
        public DrawChoice Choice { get; private set; } = DrawChoice.Keep;

        public void Release()
        {
            Movement = default;
            TriggerPulled = false;
            ReloadPressed = false;
            Choice = DrawChoice.Keep;
        }

        public void Steer(MovementInput input) =>
            Movement = input;

        public void AimAt(float angle) =>
            Aim = angle;

        public void PullTrigger() =>
            TriggerPulled = true;

        public void PressReload() =>
            ReloadPressed = true;

        public void Choose(DrawChoice choice) =>
            Choice = choice;
    }
}

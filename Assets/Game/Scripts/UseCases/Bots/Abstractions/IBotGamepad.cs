using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Movement;

namespace Bw.UseCases.Bots.Abstractions
{
    public interface IReadonlyBotGamepad
    {
        public MovementInput Movement { get; }
        public float Aim { get; }
        public bool TriggerPulled { get; }
        public bool ReloadPressed { get; }
        public DrawChoice Choice { get; }
    }

    public interface IBotGamepad : IReadonlyBotGamepad
    {
        public void Release();
        public void Steer(MovementInput input);
        public void AimAt(float angle);
        public void PullTrigger();
        public void PressReload();
        public void Choose(DrawChoice choice);
    }
}

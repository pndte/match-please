using Bw.UseCases.Combat.Hands;
using Bw.UseCases.Movement;
using UnityEngine;

namespace Bw.UseCases.Bots.Abstractions
{
    public interface IReadonlyBotGamepad
    {
        public MovementInput Movement { get; }
        public Vector2 AimPoint { get; }
        public bool TriggerPulled { get; }
        public bool ReloadPressed { get; }
        public DrawChoice Choice { get; }
    }

    public interface IBotGamepad : IReadonlyBotGamepad
    {
        public void Release();
        public void Steer(MovementInput input);
        public void AimAt(Vector2 point);
        public void PullTrigger();
        public void PressReload();
        public void Choose(DrawChoice choice);
    }
}

using Bw.UseCases.Character.View.Player;
using Bw.UseCases.Mortality.View.Guarding;
using Bw.UseCases.Mortality.View.Health;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.Abstractions
{
    public interface IHealthBars
    {
        public void Show(
            Lifetime lifetime,
            IHealthViewModel health,
            IPlayerViewModel player,
            Transform anchor);

        public void ShowGuard(Lifetime lifetime, IGuardViewModel guard, Transform anchor);
    }
}

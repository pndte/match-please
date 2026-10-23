using Bw.UseCases.Character.View.Abstractions;
using Bw.UseCases.Mortality.View.Guarding;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.HealthBar
{
    public sealed class CharacterGuardBar
    {
        public CharacterGuardBar(
            Lifetime lifetime,
            IHealthBars bars,
            IGuardViewModel guard,
            Transform body) =>
            bars.ShowGuard(lifetime, guard, body);
    }
}

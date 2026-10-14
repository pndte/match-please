using Bw.UseCases.Character.View.Abstractions;
using Bw.UseCases.Character.View.Player;
using Bw.UseCases.Mortality.View.Health;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.HealthBar
{
    public sealed class CharacterHealthBar
    {
        public CharacterHealthBar(
            Lifetime lifetime,
            IHealthBars bars,
            IHealthViewModel health,
            IPlayerViewModel player,
            Transform body) =>
            bars.Show(lifetime, health, player, body);
    }
}

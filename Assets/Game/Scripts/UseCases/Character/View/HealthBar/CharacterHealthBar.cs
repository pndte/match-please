using Bw.Entities;
using Bw.UseCases.Character.View.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.HealthBar
{
    public sealed class CharacterHealthBar
    {
        public CharacterHealthBar(
            Lifetime lifetime,
            IHealthBars bars,
            IReadonlyHealth health,
            IOwnership ownership,
            IReadonlyNickname nickname,
            Transform body) =>
            bars.Show(lifetime, health, ownership, nickname, body);
    }
}

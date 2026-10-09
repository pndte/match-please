using Bw.Entities.Extensions;
using Bw.UseCases.Character.Extensions;
using Bw.UseCases.Vfx.View.Effects.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.Death
{
    public sealed class CharacterDeathBurst
    {
        private readonly Transform _body;
        private readonly IEffectPlayer _effects;
        private readonly CharacterDeathViewConfig _config;

        public CharacterDeathBurst(
            Lifetime lifetime,
            IReadonlyCharacter character,
            Transform body,
            IEffectPlayer effects,
            CharacterDeathViewConfig config)
        {
            _body = body;
            _effects = effects;
            _config = config;

            character.AdviseKilled(lifetime, () => lifetime.WhenElapsed(config.BurstTime, Burst));
        }

        private void Burst() =>
            _effects.Play(_config.Blood, (Vector2)_body.position + _config.BurstOffset, Vector2.up);
    }
}

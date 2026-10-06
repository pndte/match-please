using Bw.Entities;
using Bw.Entities.Extensions;
using JetBrains.Lifetimes;
using R3;
using UnityEngine;

namespace Bw.UseCases.Character.View.Hit
{
    public sealed class HitFlash
    {
        private static readonly int FlashColor = Shader.PropertyToID("_FlashColor");
        private static readonly int FlashAmount = Shader.PropertyToID("_FlashAmount");

        private readonly SpriteRenderer _sprite;
        private readonly HitFeedbackConfig _config;
        private readonly SequentialLifetimes _flashes;
        private readonly MaterialPropertyBlock _properties = new();

        private float _elapsed;

        public HitFlash(Lifetime lifetime, IReadonlyHealth health, SpriteRenderer sprite, HitFeedbackConfig config)
        {
            _sprite = sprite;
            _config = config;
            _flashes = new SequentialLifetimes(lifetime);

            health.AdviseDamage(lifetime, _ => Flash()); //TODO: опоздавшему клиенту здоровье приходит отдельным сообщением уже после создания персонажа — раненый до его входа персонаж мигнёт один раз
        }

        private void Flash()
        {
            _elapsed = 0f;
            Show(_config.FlashStrength);
            Observable.EveryUpdate(UnityFrameProvider.Update, _flashes.Next()).Subscribe(Fade);
        }

        private void Fade(Unit _)
        {
            _elapsed += Time.deltaTime;
            var remaining = Mathf.Max(0f, 1f - _elapsed / _config.FlashDuration);
            Show(_config.FlashStrength * remaining * remaining);
            if (remaining == 0f)
                _flashes.TerminateCurrent();
        }

        private void Show(float amount)
        {
            _sprite.GetPropertyBlock(_properties);
            _properties.SetColor(FlashColor, _config.FlashColor);
            _properties.SetFloat(FlashAmount, amount);
            _sprite.SetPropertyBlock(_properties);
        }
    }
}

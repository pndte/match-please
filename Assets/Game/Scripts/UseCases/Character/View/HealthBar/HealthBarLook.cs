using Bw.UseCases.Mortality.View.Health;
using DG.Tweening;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.HealthBar
{
    public sealed class HealthBarLook
    {
        private static readonly int FillId = Shader.PropertyToID("_Fill");
        private static readonly int TrailId = Shader.PropertyToID("_Trail");
        private static readonly int HealFromId = Shader.PropertyToID("_HealFrom");
        private static readonly int HealId = Shader.PropertyToID("_Heal");
        private static readonly int FlashId = Shader.PropertyToID("_Flash");
        private static readonly int LowId = Shader.PropertyToID("_Low");

        private readonly HealthBarWidget _widget;
        private readonly HealthBarConfig _config;
        private readonly MaterialChannel _fill;
        private readonly MaterialChannel _trail;
        private readonly MaterialChannel _healFrom;
        private readonly MaterialChannel _heal;
        private readonly MaterialChannel _flash;
        private readonly MaterialChannel _low;

        public HealthBarLook(
            Lifetime lifetime,
            HealthBarWidget widget,
            Material material,
            IHealthViewModel health,
            HealthBarConfig config)
        {
            _widget = widget;
            _config = config;
            _fill = new MaterialChannel(material, FillId, widget.gameObject);
            _trail = new MaterialChannel(material, TrailId, widget.gameObject);
            _healFrom = new MaterialChannel(material, HealFromId, widget.gameObject);
            _heal = new MaterialChannel(material, HealId, widget.gameObject);
            _flash = new MaterialChannel(material, FlashId, widget.gameObject);
            _low = new MaterialChannel(material, LowId, widget.gameObject);

            health.Level.Advise(lifetime, Apply);
        }

        private void Apply(HealthLevel level)
        {
            if (level.Change < 0f)
                Hurt(level.Share);
            else if (level.Change > 0f)
                Heal(level.Share);
            else
                Place(level.Share);

            _low.To(level.Share > 0f && level.Share <= _config.LowShare ? 1f : 0f, _config.LowFadeTime);
        }

        private void Hurt(float share)
        {
            var before = _fill.Value;
            _fill.Set(share);
            _trail.Set(Mathf.Max(_trail.Value, before));
            _trail.To(share, _config.TrailDrain).SetDelay(_config.TrailDelay).SetEase(Ease.InOutQuad);
            _flash.Set(1f);
            _flash.To(0f, _config.FlashTime).SetEase(Ease.OutQuad);
            _widget.Body.DOComplete();
            _widget.Body.DOShakePosition(_config.ShakeTime, new Vector3(_config.ShakeStrength, _config.ShakeStrength, 0f), _config.ShakeVibrato)
                .SetLink(_widget.gameObject);
            _widget.Body.DOPunchScale(Vector3.one * _config.HurtPunch, _config.ShakeTime, 1, 0f)
                .SetLink(_widget.gameObject);
        }

        private void Heal(float share)
        {
            _healFrom.Set(_fill.Value);
            _fill.To(share, _config.HealTime).SetEase(Ease.OutCubic);
            _trail.Set(_fill.Value);
            _trail.To(share, _config.HealTime).SetEase(Ease.OutCubic);
            _heal.Set(1f);
            _heal.To(0f, _config.HealGlowTime).SetEase(Ease.InQuad);
        }

        private void Place(float share)
        {
            _fill.Set(share);
            _trail.Set(share);
            _healFrom.Set(share);
        }
    }
}

using Bw.Entities.Extensions;
using Bw.UseCases.Character.View.Abstractions;
using Bw.UseCases.Character.View.Player;
using Bw.UseCases.Mortality.View.Health;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.HealthBar
{
    public sealed class HealthBars : MonoBehaviour, IHealthBars
    {
        private static readonly int SizeId = Shader.PropertyToID("_Size");
        private static readonly int NotchesId = Shader.PropertyToID("_Notches");
        private static readonly int SeedId = Shader.PropertyToID("_Seed");

        [SerializeField] private UnityEngine.Camera _camera;
        [SerializeField] private RectTransform _layer;
        [SerializeField] private HealthBarWidget _prefab;
        [SerializeField] private HealthBarConfig _config;

        public void Show(
            Lifetime lifetime,
            IHealthViewModel health,
            IPlayerViewModel player,
            Transform anchor)
        {
            var widget = Instantiate(_prefab, _layer);
            var shown = widget.gameObject;
            var barLifetime = lifetime.Intersect(shown.Lifetime());
            var material = new Material(widget.Bar.material);
            barLifetime.OnTermination(() =>
            {
                Destroy(shown);
                Destroy(material);
            });
            material.SetVector(SizeId, widget.Bar.rectTransform.rect.size);
            material.SetFloat(NotchesId, health.Max / _config.NotchHealth);
            material.SetFloat(SeedId, Random.Range(0f, 100f));
            widget.Bar.material = material;

            new HealthBarMotion(barLifetime, widget, anchor, _camera, _config);
            new HealthBarLook(barLifetime, widget, material, health, _config);
            new HealthBarColors(barLifetime, material, player, _config);
            new HealthBarNickname(barLifetime, widget, player);
            new HealthBarPresence(barLifetime, widget, health, _config);
        }
    }
}

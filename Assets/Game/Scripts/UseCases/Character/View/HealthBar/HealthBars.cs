using Bw.Entities.Extensions;
using Bw.UseCases.Character.View.Abstractions;
using Bw.UseCases.Character.View.Player;
using Bw.UseCases.Mortality.View.Guarding;
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
        private static readonly int FillColorId = Shader.PropertyToID("_FillColor");
        private static readonly int FillShadeId = Shader.PropertyToID("_FillShade");

        [SerializeField] private UnityEngine.Camera _camera;
        [SerializeField] private RectTransform _layer;
        [SerializeField] private HealthBarWidget _prefab;
        [SerializeField] private HealthBarConfig _config;
        [SerializeField] private HealthBarConfig _guardConfig;

        public void Show(
            Lifetime lifetime,
            IHealthViewModel health,
            IPlayerViewModel player,
            Transform anchor)
        {
            var widget = Create(lifetime, health.Max, _config, out var barLifetime, out var material);
            new HealthBarFollower(barLifetime, widget, anchor, _camera, _config);
            new HealthBarMaterialView(barLifetime, widget, material, health, _config);
            new HealthBarColors(barLifetime, material, player, _config);
            new HealthBarNickname(barLifetime, widget, player);
            new HealthBarPresenceView(barLifetime, widget, health, _config);
        }

        public void ShowGuard(Lifetime lifetime, IGuardViewModel guard, Transform anchor) //TODO: значок щита у полоски блока: нужен рисунок
        {
            var widget = Create(lifetime, guard.Max, _guardConfig, out var barLifetime, out var material);
            material.SetColor(FillColorId, _guardConfig.OwnFill);
            material.SetColor(FillShadeId, _guardConfig.OwnShade);
            widget.Nickname.gameObject.SetActive(false);
            widget.Group.alpha = 0f;
            new HealthBarFollower(barLifetime, widget, anchor, _camera, _guardConfig);
            new HealthBarMaterialView(barLifetime, widget, material, guard, _guardConfig);
            new GuardBarPresenceView(barLifetime, widget, guard, _guardConfig);
        }

        private HealthBarWidget Create(
            Lifetime lifetime,
            float max,
            HealthBarConfig config,
            out Lifetime barLifetime,
            out Material material)
        {
            var widget = Instantiate(_prefab, _layer);
            var shown = widget.gameObject;
            var created = new Material(widget.Bar.material);
            barLifetime = lifetime.Intersect(shown.Lifetime());
            barLifetime.OnTermination(() =>
            {
                Destroy(shown);
                Destroy(created);
            });
            created.SetVector(SizeId, widget.Bar.rectTransform.rect.size);
            created.SetFloat(NotchesId, max / config.NotchHealth);
            created.SetFloat(SeedId, Random.Range(0f, 100f));
            widget.Bar.material = created;
            material = created;
            return widget;
        }
    }
}

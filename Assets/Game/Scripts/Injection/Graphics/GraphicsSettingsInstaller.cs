using System;
using System.Collections.Generic;
using Bw.Entities.Extensions;
using Bw.UseCases.Graphics;
using Bw.UseCases.Graphics.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

namespace Bw.Injection.Graphics
{
    [CreateAssetMenu(fileName = "GraphicsSettings", menuName = "Installers/GraphicsSettings")]
    public sealed class GraphicsSettingsInstaller : ScriptableObjectInstaller<GraphicsSettingsInstaller>
    {
        [SerializeField] private GraphicsPresetsConfig _presets;

        public override void InstallBindings()
        {
            RequirePresets();

            BindQuality();
            BindQualityEffects();
        }

        private void BindQuality()
        {
            Container.BindInstance(_presets).AsSingle();
            Container.Bind<GraphicsPresetPrefs>().AsSingle();
            Container.Bind(typeof(IReadonlyGraphicsQuality), typeof(IGraphicsQuality)).To<GraphicsQuality>()
                .FromMethod(context => new GraphicsQuality(_presets, context.Container.Resolve<GraphicsPresetPrefs>().Saved))
                .AsSingle();
            Container.Bind<GraphicsPresetSaver>().AsSingle().NonLazy();
        }

        private void BindQualityEffects()
        {
            Container.Bind<Lifetime>().FromMethod(() => Container.Resolve<Context>().gameObject.Lifetime()).AsSingle()
                .WhenInjectedInto(typeof(GraphicsPresetSaver), typeof(PostQuality), typeof(RenderPipelineQuality));
            Container.Bind<Volume>().FromNewComponentOnNewGameObject().WithGameObjectName("Graphics Quality").AsSingle()
                .OnInstantiated<Volume>((_, volume) =>
                {
                    volume.isGlobal = true;
                    volume.priority = 100f;
                })
                .WhenInjectedInto<PostQuality>();
            Container.Bind<PostQuality>().AsSingle().NonLazy();
            Container.Bind<RenderPipelineQuality>().AsSingle().NonLazy();
        }

        private void RequirePresets()
        {
            if (_presets == null || _presets.Presets == null || _presets.Presets.Length == 0)
                throw new InvalidOperationException($"Graphics settings '{name}' have no presets: set a GraphicsPresetsConfig with at least one preset.");

            if (Array.IndexOf(_presets.Presets, _presets.Default) < 0)
                throw new InvalidOperationException($"Graphics presets '{_presets.name}' default to a preset that is not among them.");

            var names = new HashSet<string>();
            foreach (var preset in _presets.Presets)
            {
                if (preset == null || string.IsNullOrWhiteSpace(preset.Title) || preset.Pipeline == null || preset.PostOverrides == null)
                    throw new InvalidOperationException(
                        $"Graphics presets '{_presets.name}' have a preset without a title, a pipeline or post overrides: every preset needs all three, an empty profile keeps the scene's own post-processing.");

                if (!names.Add(preset.name))
                    throw new InvalidOperationException($"Graphics presets '{_presets.name}' list '{preset.name}' twice: the saved choice is found by the preset's name.");
            }
        }
    }
}

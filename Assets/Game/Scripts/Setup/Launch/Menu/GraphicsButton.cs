using System;
using Bw.UseCases.Graphics;
using Bw.UseCases.Graphics.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine.UIElements;

namespace Setup.Launch.Menu
{
    public sealed class GraphicsButton
    {
        public const string ButtonName = "graphics";

        private readonly IGraphicsQuality _quality;
        private readonly GraphicsPresetsConfig _presets;

        public GraphicsButton(
            Lifetime lifetime,
            MenuDocument document,
            IGraphicsQuality quality,
            GraphicsPresetsConfig presets)
        {
            _quality = quality;
            _presets = presets;

            var button = document.Layer("main-menu-layer").Q<Button>(ButtonName);
            quality.Preset.Advise(lifetime, preset => button.text = $"Графика: {preset.Title}");
            button.AdviseClicked(lifetime, SelectNext);
        }

        private void SelectNext()
        {
            var index = Array.IndexOf(_presets.Presets, _quality.Preset.Value);
            _quality.Select(_presets.Presets[(index + 1) % _presets.Presets.Length]);
        }
    }
}

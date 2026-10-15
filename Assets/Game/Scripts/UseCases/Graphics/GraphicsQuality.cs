using System;
using Bw.UseCases.Graphics.Abstractions;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Graphics
{
    public sealed class GraphicsQuality : IGraphicsQuality
    {
        public IReadonlyProperty<GraphicsPreset> Preset => _preset;

        private readonly ViewableProperty<GraphicsPreset> _preset;
        private readonly GraphicsPresetsConfig _presets;

        public GraphicsQuality(GraphicsPresetsConfig presets, GraphicsPreset initial)
        {
            _presets = presets;
            _preset = new ViewableProperty<GraphicsPreset>(initial);
        }

        public void Select(GraphicsPreset preset)
        {
            if (Array.IndexOf(_presets.Presets, preset) < 0)
                throw new ArgumentException($"Graphics preset '{preset.name}' is not one of '{_presets.name}'.", nameof(preset));

            _preset.Value = preset;
        }
    }
}

using Bw.UseCases.Graphics.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Graphics
{
    public sealed class GraphicsPresetSaver
    {
        public GraphicsPresetSaver(Lifetime lifetime, IReadonlyGraphicsQuality quality, GraphicsPresetPrefs prefs) =>
            quality.Preset.Change.Advise(lifetime, prefs.Save);
    }
}

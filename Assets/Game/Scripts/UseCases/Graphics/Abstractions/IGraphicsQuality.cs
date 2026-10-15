using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Graphics.Abstractions
{
    public interface IReadonlyGraphicsQuality
    {
        public IReadonlyProperty<GraphicsPreset> Preset { get; }
    }

    public interface IGraphicsQuality : IReadonlyGraphicsQuality
    {
        public void Select(GraphicsPreset preset);
    }
}

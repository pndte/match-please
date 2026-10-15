using Bw.UseCases.Graphics.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Graphics
{
    public sealed class RenderPipelineQuality
    {
        public RenderPipelineQuality(Lifetime lifetime, IReadonlyGraphicsQuality quality)
        {
            var original = QualitySettings.renderPipeline;
            lifetime.OnTermination(() => QualitySettings.renderPipeline = original);
            quality.Preset.Advise(lifetime, preset => QualitySettings.renderPipeline = preset.Pipeline);
        }
    }
}

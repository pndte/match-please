using Bw.UseCases.Graphics.Abstractions;
using JetBrains.Lifetimes;
using UnityEngine.Rendering;

namespace Bw.UseCases.Graphics
{
    public sealed class PostQuality
    {
        public PostQuality(Lifetime lifetime, IReadonlyGraphicsQuality quality, Volume volume) =>
            quality.Preset.Advise(lifetime, preset => volume.sharedProfile = preset.PostOverrides);
    }
}

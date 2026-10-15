using Bw.Entities.Extensions;
using Bw.UseCases.Graphics.Abstractions;
using Bw.UseCases.Level.View.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Level.View.Adapters
{
    public sealed class LampHazeViewModel : ILampHazeViewModel
    {
        public IReadonlyProperty<bool> Live { get; }

        public LampHazeViewModel(Lifetime lifetime, IReadonlyGraphicsQuality quality) =>
            Live = quality.Preset.Select(lifetime, preset => preset.LiveLampHaze);
    }
}

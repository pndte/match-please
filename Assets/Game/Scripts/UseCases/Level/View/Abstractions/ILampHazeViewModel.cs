using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Level.View.Abstractions
{
    public interface ILampHazeViewModel
    {
        public IReadonlyProperty<bool> Live { get; }
    }
}

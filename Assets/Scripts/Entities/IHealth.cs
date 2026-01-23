using JetBrains.Collections.Viewable;

namespace Bw.Entities
{
    public interface IReadonlyHealth : IReadonlyProperty<float>
    {
        public float Max { get; }
    }

    public interface IHealth : IReadonlyHealth, IViewableProperty<float>
    {
    }
}
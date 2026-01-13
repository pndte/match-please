using JetBrains.Collections.Viewable;

namespace Entities
{
    public interface IReadonlyHealth : IReadonlyProperty<float>
    {
        public float MaxHealth { get; }
    }

    public interface IHealth : IReadonlyHealth, IViewableProperty<float>
    {
    }
}
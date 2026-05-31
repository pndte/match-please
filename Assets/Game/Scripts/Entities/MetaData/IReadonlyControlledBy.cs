using JetBrains.Collections.Viewable;

namespace Bw.Entities
{
    public interface IReadonlyControlledBy //TODO: rename to IControlledBy, norm name for IControllerBy
    {
        public IReadonlyProperty<bool> Me { get; }
    }
}
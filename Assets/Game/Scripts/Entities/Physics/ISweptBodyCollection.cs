using JetBrains.Collections.Viewable;

namespace Bw.Entities.Physics
{
    public interface ISweptBodyCollection : IViewableList<SweptBody>
    {
    }

    public sealed class SweptBodyCollection : ViewableList<SweptBody>, ISweptBodyCollection
    {
    }
}

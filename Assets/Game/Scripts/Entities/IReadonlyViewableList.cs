using System.Collections.Generic;
using JetBrains.Collections.Viewable;

namespace Bw.Entities
{
    public interface IReadonlyViewableList<T> : IReadOnlyList<T>, ISource<ListEvent<T>>
    {
        ISource<ListEvent<T>> Change { get; } 
    }
}
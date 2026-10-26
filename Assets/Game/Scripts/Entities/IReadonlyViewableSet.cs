using System.Collections.Generic;
using JetBrains.Collections.Viewable;

namespace Bw.Entities
{
    public interface IReadonlyViewableSet<T> : IReadOnlyCollection<T>, ISource<SetEvent<T>>
    {
        public bool Contains(T item);
    }
}

using System.Collections.Generic;
using JetBrains.Collections.Viewable;

namespace Bw.Entities
{
    public interface IReadonlyViewableMap<K, V> : IReadOnlyDictionary<K, V>, ISource<MapEvent<K, V>>
    {
    }
}

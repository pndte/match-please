using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Bw.Entities.Network.Prediction.Events
{
    internal sealed class SameObjectComparer : IEqualityComparer<object>
    {
        bool IEqualityComparer<object>.Equals(object x, object y) =>
            ReferenceEquals(x, y);

        int IEqualityComparer<object>.GetHashCode(object obj) =>
            RuntimeHelpers.GetHashCode(obj);
    }
}

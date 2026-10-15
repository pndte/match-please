using System;
using Bw.Entities;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases
{
    public sealed class Ownership : IOwnership
    {
        public IReadonlyProperty<bool> Mine => _mine;

        private readonly IViewableProperty<bool> _mine = new ViewableProperty<bool>(false);

        public void SetMine(Lifetime lifetime)
        {
            if (_mine.Value)
                throw new InvalidOperationException("The object is already mine: SetMine gives ownership for one lifetime, and the previous one has not ended yet.");

            _mine.Value = true;
            lifetime.OnTermination(() => _mine.Value = false);
        }
    }
}

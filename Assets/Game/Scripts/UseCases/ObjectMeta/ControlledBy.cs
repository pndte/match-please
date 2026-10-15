using System;
using Bw.Entities;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases
{
    public sealed class ControlledBy : IControlledBy
    {
        public IReadonlyProperty<bool> Me => _me;

        private readonly IViewableProperty<bool> _me = new ViewableProperty<bool>(false);

        public void SetMe(Lifetime lifetime)
        {
            if (_me.Value)
                throw new InvalidOperationException("The object is already controlled by me: SetMe gives control for one lifetime, and the previous one has not ended yet.");

            _me.Value = true;
            lifetime.OnTermination(() => _me.Value = false);
        }
    }
}

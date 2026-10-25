using Bw.Entities;
using Bw.Entities.Network.Rollback;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.Entities.Network.Prediction.Doubles
{
    public sealed class Toggle : IPredictedBy, IReadonlyControlledBy
    {
        public IReadonlyProperty<bool> Me => _me;

        private readonly ViewableProperty<bool> _me;

        public Toggle(bool on = false)
        {
            _me = new ViewableProperty<bool>(on);
        }

        public void Set(bool on) =>
            _me.Value = on;
    }
}

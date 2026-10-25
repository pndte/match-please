using Bw.Entities;
using Bw.Entities.Network.Rollback;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.UseCases.Combat.Weapon.Doubles
{
    public sealed class ManualControl : IReadonlyControlledBy, IPredictedBy
    {
        public IReadonlyProperty<bool> Me => _me;

        private readonly ViewableProperty<bool> _me;

        public ManualControl(bool me = false)
        {
            _me = new ViewableProperty<bool>(me);
        }

        public void Take() =>
            _me.Value = true;

        public void Release() =>
            _me.Value = false;
    }
}

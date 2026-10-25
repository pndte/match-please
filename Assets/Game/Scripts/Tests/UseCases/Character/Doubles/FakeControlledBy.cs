using Bw.Entities;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.UseCases.Character.Doubles
{
    public sealed class FakeControlledBy : IReadonlyControlledBy
    {
        public IReadonlyProperty<bool> Me => _me;

        private readonly ViewableProperty<bool> _me;

        public FakeControlledBy(bool me = false)
        {
            _me = new ViewableProperty<bool>(me);
        }

        public void Set(bool me) =>
            _me.Value = me;
    }
}

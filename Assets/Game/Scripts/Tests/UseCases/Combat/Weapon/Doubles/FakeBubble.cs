using Bw.Entities.Network.Prediction.Bubble;
using JetBrains.Collections.Viewable;

namespace Bw.Tests.UseCases.Combat.Weapon.Doubles
{
    public sealed class FakeBubble : IBubbleMembership
    {
        public IReadonlyProperty<bool> Inside => _inside;

        private readonly ViewableProperty<bool> _inside;

        public FakeBubble(bool inside = false)
        {
            _inside = new ViewableProperty<bool>(inside);
        }

        public void Enter() =>
            _inside.Value = true;

        public void Leave() =>
            _inside.Value = false;
    }
}

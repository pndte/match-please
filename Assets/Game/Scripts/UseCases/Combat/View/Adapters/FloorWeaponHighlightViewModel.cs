using Bw.UseCases.Combat.View.Highlight;
using Bw.UseCases.Combat.Weapon.Pickup.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.View.Adapters
{
    public sealed class FloorWeaponHighlightViewModel : IWeaponHighlight, IHighlightViewModel
    {
        public IReadonlyProperty<bool> Highlighted => _highlighted;

        private readonly ViewableProperty<bool> _highlighted = new(false);

        private int _shown;

        public void Show(Lifetime highlightLifetime)
        {
            _shown++;
            _highlighted.Value = true;
            highlightLifetime.OnTermination(() => _highlighted.Value = --_shown > 0);
        }
    }
}

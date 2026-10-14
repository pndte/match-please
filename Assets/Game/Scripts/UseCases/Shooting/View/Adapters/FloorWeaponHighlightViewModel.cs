using Bw.UseCases.Shooting.View.Highlight;
using Bw.UseCases.Shooting.Weapon.Pickup.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.View.Adapters
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

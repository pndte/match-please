using Bw.UseCases.Shooting.View.Abstractions;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Shooting.View.Adapters
{
    public sealed class FlashlightViewModel : IFlashlightViewModel
    {
        public IReadonlyProperty<bool> Lit => _flashlight.Lit;

        private readonly IReadonlyFlashlight _flashlight;

        public FlashlightViewModel(IReadonlyFlashlight flashlight) =>
            _flashlight = flashlight;
    }
}

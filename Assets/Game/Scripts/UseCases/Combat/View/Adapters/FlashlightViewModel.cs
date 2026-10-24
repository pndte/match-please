using Bw.UseCases.Combat.Shooting.Abstractions;
using Bw.UseCases.Combat.View.Abstractions;
using JetBrains.Collections.Viewable;

namespace Bw.UseCases.Combat.View.Adapters
{
    public sealed class FlashlightViewModel : IFlashlightViewModel
    {
        public IReadonlyProperty<bool> Lit => _flashlight.Lit;

        private readonly IReadonlyFlashlight _flashlight;

        public FlashlightViewModel(IReadonlyFlashlight flashlight) =>
            _flashlight = flashlight;
    }
}

using Bw.Entities.Network.Variables;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network.Requests
{
    public sealed class FlashlightServerHandler
    {
        public FlashlightServerHandler(
            Lifetime lifetime,
            INetRequestReceiver<bool> @switch,
            IFlashlight flashlight) =>
            @switch.Received.Advise(lifetime, flashlight.Apply);
    }
}

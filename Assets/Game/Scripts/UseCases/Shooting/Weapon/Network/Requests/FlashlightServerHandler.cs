using Bw.Entities.Network.Variables;
using Bw.UseCases.Shooting.Weapon.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Shooting.Weapon.Network.Requests
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

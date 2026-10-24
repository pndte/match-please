using Bw.Entities.Network.Variables;
using Bw.UseCases.Combat.Shooting.Abstractions;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Shooting.Network.Requests
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

using JetBrains.Lifetimes;

namespace Bw.UseCases.Character.Network.Requests
{
    public sealed class VitalsBroadcaster
    {
        public VitalsBroadcaster(Lifetime lifetime, IReadonlyCharacter character, IVitalsResult vitals)
        {
            character.Applied.Advise(lifetime, vitals.Received.Fire);
        }
    }
}

using JetBrains.Lifetimes;

namespace Bw.UseCases.Character.Network.Requests
{
    public sealed class VitalsReceiver
    {
        public VitalsReceiver(Lifetime lifetime, IVitalsResult vitals, ICharacter character)
        {
            vitals.Received.Advise(lifetime, character.Apply);
        }
    }
}

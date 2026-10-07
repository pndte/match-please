using Bw.Entities.Network.Variables;

namespace Bw.UseCases.Character.Network.Requests
{
    public sealed class CharacterSignals : IVitalsResult
    {
        INetSignal<CharacterVitals> IVitalsResult.Received => _vitals;

        private readonly INetSignal<CharacterVitals> _vitals;

        public CharacterSignals(INetSignal<CharacterVitals> vitals)
        {
            _vitals = vitals;
        }
    }
}

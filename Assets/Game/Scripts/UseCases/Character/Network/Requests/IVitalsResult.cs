using Bw.Entities.Network.Variables;

namespace Bw.UseCases.Character.Network.Requests
{
    public interface IVitalsResult
    {
        public INetSignal<CharacterVitals> Received { get; }
    }
}

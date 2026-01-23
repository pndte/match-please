using Bw.UseCases.Character.Network;

namespace Bw.UseCases
{
    public interface IPlayer
    {
        public NetworkCharacter ICharacter { get; }
    }
}
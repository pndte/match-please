using Bw.Entities.Network.Variables;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Character.Network
{
    public sealed class NicknameReceiver
    {
        public NicknameReceiver(Lifetime lifetime, INetEntries entries, NetPropertyDeclaration<string> declaration, INickname nickname) =>
            entries.Get(declaration).Advise(lifetime, nickname.Apply);
    }
}

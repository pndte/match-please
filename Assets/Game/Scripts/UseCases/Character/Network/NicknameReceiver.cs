using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Character.Network
{
    public sealed class NicknameReceiver
    {
        public NicknameReceiver(Lifetime lifetime, IViewableProperty<string> received, INickname nickname) =>
            received.Advise(lifetime, nickname.Apply);
    }
}

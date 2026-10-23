using Bw.Entities.Network.Variables;
using Bw.UseCases.Mortality.Guarding;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Mortality.Network.Guarding
{
    public sealed class GuardReceiver //TODO: свой поднятый блок виден только после ответа сервера, хотя замедление предсказано сразу: стойку из своего ввода можно показывать без ожидания
    {
        public GuardReceiver(
            Lifetime lifetime,
            INetEntries entries,
            NetPropertyDeclaration<GuardState> declaration,
            IGuard guard) =>
            entries.Get(declaration).Advise(lifetime, guard.Apply);
    }
}

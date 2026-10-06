using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Simulation;
using Zenject;

namespace Bw.Injection.Network
{
    public class LagCompensationInstaller<TState, TView> : Installer<IRuntimeSettings, LagCompensationInstaller<TState, TView>> //TODO: движущиеся объекты без ввода игрока (ящики, которые пинают или которые едут сами) — без симуляции модуля предикции: нужен его вариант без ввода (сервер надёжно шлёт изменения состояния с номером тика, клиент показывает их на InterpolationTick, как чужих персонажей) — тогда откат подключается здесь же, одной строкой. Неподвижным разрушаемым (стенам) откат не нужен, пока они только исчезают: без него сервер ошибается лишь в пользу стрелка
        where TState : struct
        where TView : class, IStateView<TState>
    {
        private readonly IRuntimeSettings _runtimeSettings;

        public LagCompensationInstaller(IRuntimeSettings runtimeSettings)
        {
            _runtimeSettings = runtimeSettings;
        }

        public override void InstallBindings()
        {
            if (_runtimeSettings.CurrentPeerType != PeerType.Server)
                return;

            Container.Bind<SimulationRewinder<TState>>()
                .FromMethod(context => context.Container.Instantiate<SimulationRewinder<TState>>(
                    new object[] { context.Container.Instantiate(typeof(TView)) }))
                .AsSingle();
            Container.Bind<RewindableRegistration>()
                .FromMethod(context => context.Container.Instantiate<RewindableRegistration>(
                    new object[] { context.Container.Resolve<SimulationRewinder<TState>>() }))
                .AsSingle()
                .NonLazy();
        }
    }
}

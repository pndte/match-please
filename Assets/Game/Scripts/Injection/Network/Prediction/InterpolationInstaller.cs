using Bw.Entities.Network.Prediction.Stream;
using Zenject;

namespace Bw.Injection.Network.Prediction
{
    public class InterpolationInstaller<TState> : Installer<InterpolationInstaller<TState>>
        where TState : struct
    {
        public override void InstallBindings()
        {
            Container.Bind<PredictionInterpolation<TState>>().AsSingle().NonLazy();
            Container.Bind<SnapshotInterpolation<TState>>().AsSingle().NonLazy();
        }
    }
}

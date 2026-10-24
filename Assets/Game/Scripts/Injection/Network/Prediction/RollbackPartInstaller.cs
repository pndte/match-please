using System;
using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Replication;
using Bw.Entities.Network.Rollback;
using Zenject;

namespace Bw.Injection.Network.Prediction
{
    public class RollbackPartInstaller<TState> : Installer<RollbackPartInstaller<TState>>
        where TState : struct, IEquatable<TState>
    {
        public override void InstallBindings()
        {
            Container.Bind<IReplicatedStates<TState>>().To<ReplicatedStates<TState>>().AsSingle().NonLazy();
            Container.Bind<RollbackPart<TState>>().AsSingle();
            Container.Bind<RollbackPartRegistration<TState>>().AsSingle().NonLazy();
            Container.Bind<ServerStateFollower<TState>>().AsSingle().NonLazy();
            Container.Bind<IStateCorrections<TState>>().To<StateCorrections<TState>>().AsSingle();
        }
    }
}

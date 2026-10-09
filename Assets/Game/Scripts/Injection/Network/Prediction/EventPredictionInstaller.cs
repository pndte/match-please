using System;
using Bw.Entities.Network;
using Bw.Entities.Network.Prediction.Events;
using Zenject;

namespace Bw.Injection.Network.Prediction
{
    public class EventPredictionInstaller : Installer<EventPredictionInstaller>
    {
        [Inject] private IRuntimeSettings _runtimeSettings;

        public override void InstallBindings()
        {
            switch (_runtimeSettings.CurrentPeerType)
            {
                case PeerType.Server:
                    Container.Bind(typeof(IAffectables<>)).To(typeof(Affectables<>)).AsSingle();
                    break;
                case PeerType.Client:
                    Container.Bind(typeof(IPredictionTargets<>)).To(typeof(PredictionTargets<>)).AsSingle();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}

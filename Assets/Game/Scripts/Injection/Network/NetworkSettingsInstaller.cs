using Bw.Entities.Network;
using Bw.Entities.Network.LagCompensation;
using Bw.Entities.Network.Prediction.Events;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Movement.Network.Prediction;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Network
{
    [CreateAssetMenu(fileName = "NetworkSettings", menuName = "Installers/NetworkSettings")]
    public sealed class NetworkSettingsInstaller : ScriptableObjectInstaller<NetworkSettingsInstaller>
    {
        [SerializeField] private ConnectionConfig _connection = new();
        [SerializeField] private NetworkTicksConfig _ticks = new();
        [SerializeField] private LagCompensationConfig _lagCompensation = new();
        [SerializeField] private EventPredictionConfig _eventPrediction = new();
        [SerializeField] private PushPredictionConfig _pushPrediction = new();

        public override void InstallBindings()
        {
            Container.BindInstance(_connection).AsSingle();
            Container.BindInstance(_ticks).AsSingle();
            Container.BindInstance(_lagCompensation).AsSingle();
            Container.BindInstance(_eventPrediction).AsSingle();
            Container.BindInstance(_pushPrediction).AsSingle();
        }
    }
}

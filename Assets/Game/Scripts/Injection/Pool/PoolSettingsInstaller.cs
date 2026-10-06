using Bw.Entities.Pool.GameObjects;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Pool
{
    [CreateAssetMenu(fileName = "PoolSettings", menuName = "Installers/PoolSettings")]
    public sealed class PoolSettingsInstaller : ScriptableObjectInstaller<PoolSettingsInstaller>
    {
        [SerializeField] private PrefabPoolsConfig _pools = new();

        public override void InstallBindings()
        {
            Container.BindInstance(_pools).AsSingle();
        }
    }
}

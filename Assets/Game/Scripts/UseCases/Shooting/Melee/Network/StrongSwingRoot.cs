using Bw.Entities.Extensions;
using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Movement.Network.Root;
using Bw.UseCases.Shooting.Melee.Extensions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Shooting.Melee.Network
{
    public sealed class StrongSwingRoot
    {
        private readonly Transform _weapon;
        private readonly INetworkTicks _ticks;
        private readonly MeleeWeaponConfig _config;

        public StrongSwingRoot(
            Lifetime lifetime,
            Transform weapon,
            IReadonlySimulation<MeleeState> fists,
            INetworkTicks ticks,
            MeleeWeaponConfig config)
        {
            _weapon = weapon;
            _ticks = ticks;
            _config = config;

            fists.Stepped.Advise(lifetime, Root);
        }

        private void Root(MeleeState state)
        {
            if (!state.StrongSwingStarted())
                return;

            var tick = _ticks.Current;
            _weapon.HolderService<IMovementRoot>().Root(tick + 1, tick + _config.StrongSwingTicks(_ticks) + _config.RecoveryTicks(_ticks)); //TODO: корень ставится по тику, в котором удар сорвался в предсказании; если сверка с сервером сдвинет этот тик (потерялся ввод с отпусканием кнопки), корень клиента останется на старых тиках — сервер держит свой корень и поправит позицию рывком
        }
    }
}

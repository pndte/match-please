using Bw.Entities.Network.Ticks;
using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Hands;
using Bw.UseCases.Combat.Abstractions.Hands.Extensions;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Melee.Extensions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Melee.Network
{
    public sealed class StrongSwingRoot
    {
        private readonly IWeapon _fists;
        private readonly IWeaponsInHands _inHands;
        private readonly IMovementRoots _roots;
        private readonly INetworkTicks _ticks;
        private readonly MeleeWeaponConfig _config;

        public StrongSwingRoot( //TODO: чрезвычайное к-во параметров
            Lifetime lifetime,
            IWeapon fists,
            IReadonlySimulation<MeleeState> swings,
            IWeaponsInHands inHands,
            IMovementRoots roots,
            INetworkTicks ticks,
            MeleeWeaponConfig config)
        {
            _fists = fists;
            _inHands = inHands;
            _roots = roots;
            _ticks = ticks;
            _config = config;

            swings.Stepped.Advise(lifetime, Root);
        }

        private void Root(MeleeState state)
        {
            if (!state.StrongSwingStarted())
                return;

            var tick = _ticks.Current;
            _roots.ByCharacter[_inHands.HolderOf(_fists)].Root(tick + 1, tick + _config.StrongSwingTicks(_ticks) + _config.RecoveryTicks(_ticks)); //TODO: корень ставится по тику, в котором удар сорвался в предсказании; если сверка с сервером сдвинет этот тик (потерялся ввод с отпусканием кнопки), корень клиента останется на старых тиках — сервер держит свой корень и поправит позицию рывком
        }
    }
}

using Bw.Entities.Network.Prediction.Stream;
using Bw.Entities.Network.Prediction.Stream.Requests;
using Bw.Entities.Network.Ticks;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon.Abstractions;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.Weapon.Network
{
    public sealed class WeaponFeedLog : IFedInputs<WeaponInput>
    {
        public ISource<TickedInput<WeaponInput>> Fed => _fed;

        private readonly Signal<TickedInput<WeaponInput>> _fed = new();
        private readonly INetworkTicks _ticks;

        private WeaponInput _previous;

        public WeaponFeedLog(Lifetime lifetime, IWeaponFeed feed, INetworkTicks ticks)
        {
            _ticks = ticks;

            feed.Fed.Advise(lifetime, Log);
        }

        private void Log(WeaponInput input)
        {
            var ticked = new TickedInput<WeaponInput>(_ticks.Current, input, _previous);
            _previous = input;
            _fed.Fire(ticked);
        }
    }
}

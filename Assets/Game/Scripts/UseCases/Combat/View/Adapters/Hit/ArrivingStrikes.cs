using Bw.UseCases.Combat.Weapon.Network.Prediction;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Combat.View.Adapters.Hit
{
    public sealed class ArrivingStrikes : IStrikes
    {
        public ISource<Strike> Struck => _struck;

        private readonly Signal<Strike> _struck = new();
        private readonly ShotLaunches _launches;
        private readonly ShotVfxConfig _vfx;

        public ArrivingStrikes(
            Lifetime lifetime,
            IStrikes strikes,
            ShotLaunches launches,
            ShotVfxConfig vfx)
        {
            _launches = launches;
            _vfx = vfx;

            strikes.Struck.Advise(lifetime, Launch);
        }

        private void Launch(Strike strike)
        {
            var from = strike.Actor.Hands.Pose.Position;
            for (var index = 0; index < strike.Affected.Count; index++)
                _launches.Launch(new ShotLaunch(strike.Affected[index].Target, from, _vfx.BulletSpeed));

            _struck.Fire(strike);
        }
    }
}

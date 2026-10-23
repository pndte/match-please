using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Characters;
using Bw.UseCases.Movement;

namespace Bw.UseCases.Props.Network
{
    public sealed class PropLauncher
    {
        private readonly IRestartable<RigidState> _body;
        private readonly ThrowLaunch _launch;
        private readonly ThrownPropHits _hits;

        public PropLauncher(IRestartable<RigidState> body, ThrowLaunch launch, ThrownPropHits hits)
        {
            _body = body;
            _launch = launch;
            _hits = hits;
        }

        public void Drop(IReadonlyCharacter holder) =>
            _body.Restart(_launch.Dropped(holder));

        public void Throw(IReadonlyCharacter holder, float aim)
        {
            _body.Restart(_launch.Thrown(holder, aim));
            _hits.Track(holder);
        }
    }
}

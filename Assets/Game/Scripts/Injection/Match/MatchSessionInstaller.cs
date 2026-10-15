using Bw.UseCases.Match;
using Bw.UseCases.Match.Network;
using Zenject;

namespace Bw.Injection.Match
{
    public sealed class MatchSessionInstaller : Installer<MatchConfig, MatchSessionInstaller>
    {
        private readonly MatchConfig _config;

        public MatchSessionInstaller(MatchConfig config)
        {
            _config = config;
        }

        public override void InstallBindings()
        {
            Container.BindInstance(_config).AsSingle();
            Container.Bind(typeof(IReadonlyScoreboard), typeof(IScoreboard)).To<Scoreboard>().AsSingle()
                .WriterOnlyInto<IScoreboard>(typeof(MatchReferee), typeof(ScoreKeeper));
            Container.BindInterfacesTo<MatchReferee>().AsSingle().NonLazy();
            Container.Bind(typeof(ScoreKeeper), typeof(IKills)).To<ScoreKeeper>().AsSingle();
            Container.Bind<Combatants>().AsSingle();
            Container.Bind<HitScoring>().AsSingle().NonLazy();
            Container.Bind<DeathScoring>().AsSingle().NonLazy();
        }
    }
}

using System;
using Bw.Entities.Players;
using Bw.UseCases.Bots;
using Bw.UseCases.Bots.Network;
using Bw.UseCases.Character;
using Bw.UseCases.Mortality;
using Bw.UseCases.Movement.Abstractions;
using JetBrains.Lifetimes;
using Zenject;

namespace Bw.Injection.Bots
{
    public sealed class BotLauncher : IBotLauncher
    {
        private readonly DiContainer _container;
        private readonly IGameObjectByCharacterCollection _characters;

        public BotLauncher(DiContainer container, IGameObjectByCharacterCollection characters)
        {
            _container = container;
            _characters = characters;
        }

        public void Launch(
            Lifetime lifetime,
            IPlayer player,
            IReadonlyMortal character,
            BotDifficultyConfig difficulty)
        {
            var life = _container.CreateSubContainer();
            life.BindInstance(lifetime);
            life.BindInstance(player);
            life.Bind<IReadonlyMortal>().FromInstance(character);
            life.Bind<IReadonlyMovement>().FromInstance(MovementOf(character));
            BotLifeInstaller.Install(life, difficulty);
            life.ResolveRoots();
        }

        private IReadonlyMovement MovementOf(IReadonlyMortal character) =>
            _characters[character].TryGetComponent<MovementHolder>(out var movement)
                ? movement.Value
                : throw new InvalidOperationException($"'{_characters[character].name}' has no MovementHolder: a bot steers its character through the character's movement.");
    }
}

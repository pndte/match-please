using System;
using Bw.UseCases;
using Bw.UseCases.Bots;
using Bw.UseCases.Bots.Network;
using Bw.UseCases.Character;
using Bw.UseCases.Combat.Abstractions.Characters;
using JetBrains.Lifetimes;
using Zenject;

namespace Bw.Injection.Bots
{
    public sealed class BotLauncher : IBotLauncher
    {
        private readonly IGameObjectByCharacterCollection _characters;
        private readonly ICharacterRegistry _registry;

        public BotLauncher(IGameObjectByCharacterCollection characters, ICharacterRegistry registry)
        {
            _characters = characters;
            _registry = registry;
        }

        public void Launch(
            Lifetime lifetime,
            IReadonlyCharacter character,
            BotDifficultyConfig difficulty,
            BotPersonality personality)
        {
            var life = ContainerOf(character).CreateSubContainer();
            life.BindInstance(lifetime);
            life.BindInstance(_registry.PlayerByCharacter[character]);
            BotLifeInstaller.Install(life, difficulty, personality);
            life.ResolveRoots();
        }

        private DiContainer ContainerOf(IReadonlyCharacter character) =>
            _characters[character].TryGetComponent<GameObjectContext>(out var context)
                ? context.Container
                : throw new InvalidOperationException($"'{_characters[character].name}' has no GameObjectContext: a bot lives in its character's container.");
    }
}

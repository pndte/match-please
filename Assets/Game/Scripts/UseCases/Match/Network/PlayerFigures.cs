using System.Collections.Generic;
using Bw.Entities.Players;
using Bw.UseCases.Character;
using Bw.UseCases.Mortality;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;

namespace Bw.UseCases.Match.Network
{
    public sealed class PlayerFigures
    {
        private const string DefaultFigure = "matvey";

        private readonly IGameObjectByCharacterCollection _gameObjects;
        private readonly Dictionary<IPlayer, string> _figures = new();

        public PlayerFigures(
            Lifetime lifetime,
            IPlayerCollection players,
            ICharacterRegistry registry,
            IGameObjectByCharacterCollection gameObjects)
        {
            _gameObjects = gameObjects;

            players.View(lifetime, (playerLifetime, player) => playerLifetime.OnTermination(() => _figures.Remove(player)));
            registry.PlayerByCharacter.View(lifetime, (_, character, player) => _figures[player] = CharacterFigure(character));
        }

        public string FigureOf(IPlayer player) =>
            _figures.GetValueOrDefault(player, DefaultFigure);

        private string CharacterFigure(IReadonlyMortal character) //TODO: фигурка по имени объекта персонажа — персонажу нужен свой ключ облика
        {
            var name = _gameObjects[character].name;
            var clone = name.IndexOf('(');
            return (clone < 0 ? name : name.Substring(0, clone)).Trim().ToLowerInvariant();
        }
    }
}

using System;
using System.Collections.Generic;
using Bw.CoreAdapters.Bots;
using Bw.Entities.Content;
using Bw.Entities.Players;
using Bw.UseCases.Bots;
using Setup.Launch;
using UnityEngine;
using Zenject;

namespace Bw.Injection.Bots
{
    [CreateAssetMenu(fileName = "RoomContent", menuName = "Installers/RoomContent")]
    public sealed class RoomContentInstaller : ScriptableObjectInstaller<RoomContentInstaller>
    {
        [SerializeField] private BotPresetCatalog _presets;
        [SerializeField] private BotPresetLooksConfig _looks;
        [SerializeField] private RoomBotsConfig _bots;
        [SerializeField] private RoomLevelsConfig _levels;

        public override void InstallBindings()
        {
            RequirePresets();
            RequireLooks();
            RequireBots();
            RequireLevels();
            Container.BindInstance(_presets).AsSingle();
            Container.BindInstance(_looks).AsSingle();
            Container.BindInstance(_bots).AsSingle();
            Container.BindInstance(_levels).AsSingle();
        }

        private void RequirePresets()
        {
            if (_presets == null || _presets.Difficulties.Length == 0 || _presets.Personalities.Length == 0)
                throw new InvalidOperationException($"Room content '{name}' needs a bot preset catalog with difficulties and personalities: assign Assets/Game/Content/Bots/BotPresetCatalog.asset.");

            if (Array.IndexOf(_presets.Difficulties, _presets.DefaultDifficulty) < 0)
                throw new InvalidOperationException($"The default difficulty of '{_presets.name}' is not among its difficulties: a new bot starts on it.");

            var keys = new HashSet<int>();
            foreach (var difficulty in _presets.Difficulties)
                RequireKey(difficulty, keys);

            keys.Clear();
            foreach (var personality in _presets.Personalities)
                RequireKey(personality, keys);
        }

        private void RequireKey(ContentAsset preset, HashSet<int> keys)
        {
            if (preset == null)
                throw new InvalidOperationException($"'{_presets.name}' has an empty row: assign a difficulty or a personality from Assets/Game/Content/Bots.");

            if (preset.Key == 0 || !keys.Add(preset.Key))
                throw new InvalidOperationException($"Preset '{preset.name}' is listed twice or has no key of its own ({preset.Key.ToString()}): the editor issues keys on import, so open the project in the main editor.");
        }

        private void RequireLooks()
        {
            if (_looks == null)
                throw new InvalidOperationException($"Room content '{name}' has no preset names: assign Assets/Game/Content/Bots/BotPresetLooksConfig.asset.");

            foreach (var difficulty in _presets.Difficulties)
                if (Array.FindIndex(_looks.Difficulties, look => look.Difficulty == difficulty && !string.IsNullOrWhiteSpace(look.DisplayName)) < 0)
                    throw new InvalidOperationException($"Difficulty '{difficulty.name}' has no name in '{_looks.name}': the bot list shows it.");

            foreach (var personality in _presets.Personalities)
                if (Array.FindIndex(_looks.Personalities, look => look.Personality == personality && !string.IsNullOrWhiteSpace(look.DisplayName)) < 0)
                    throw new InvalidOperationException($"Personality '{personality.name}' has no name in '{_looks.name}': the bot list shows it.");
        }

        private void RequireBots()
        {
            if (_bots == null)
                throw new InvalidOperationException($"Room content '{name}' has no room bots config: assign Assets/Game/Content/Bots/RoomBotsConfig.asset.");

            var names = new HashSet<string>();
            foreach (var botName in _bots.Names)
                if (!NicknameRules.IsValid(botName) || !names.Add(botName))
                    throw new InvalidOperationException($"Bot name '{botName}' in '{_bots.name}' is listed twice or no player may have it: {NicknameRules.MinLength}-{NicknameRules.MaxLength} letters, digits, spaces, '_' or '-'.");

            if (names.Count < _bots.MaxBots)
                throw new InvalidOperationException($"'{_bots.name}' lists {names.Count.ToString()} bot names for up to {_bots.MaxBots.ToString()} bots: every bot in a room needs a name of its own.");

            if (_bots.StartingBots.Length > _bots.MaxBots)
                throw new InvalidOperationException($"'{_bots.name}' starts {_bots.StartingBots.Length.ToString()} bots, more than a room holds ({_bots.MaxBots.ToString()}).");

            foreach (var bot in _bots.StartingBots)
                if (!NicknameRules.IsValid(bot.Nickname)
                    || Array.IndexOf(_presets.Difficulties, bot.Difficulty) < 0
                    || Array.IndexOf(_presets.Personalities, bot.Personality) < 0)
                    throw new InvalidOperationException($"Starting bot '{bot.Nickname}' in '{_bots.name}' needs a valid nickname and a difficulty and a personality from '{_presets.name}'.");
        }

        private void RequireLevels()
        {
            if (_levels == null || _levels.Levels.Length == 0)
                throw new InvalidOperationException($"Room content '{name}' has no levels to choose from: assign Assets/Game/Content/Setup/Room/RoomLevelsConfig.asset with at least one level.");

            foreach (var level in _levels.Levels)
                if (string.IsNullOrWhiteSpace(level.Scene) || string.IsNullOrWhiteSpace(level.DisplayName))
                    throw new InvalidOperationException($"'{_levels.name}' has a level without a scene address or a name: the room menu shows the name and loads the scene.");
        }
    }
}

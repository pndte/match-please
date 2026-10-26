using System;
using System.Collections.Generic;
using Bw.CoreAdapters.Characters;
using Bw.UseCases.Character;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    [CreateAssetMenu(fileName = "CharacterSettings", menuName = "Installers/CharacterSettings")]
    public sealed class CharacterSettingsInstaller : ScriptableObjectInstaller<CharacterSettingsInstaller>
    {
        [SerializeField] private CharacterCatalog _catalog;
        [SerializeField] private CharacterLooksConfig _looks;

        public override void InstallBindings()
        {
            RequireCatalog();
            RequireLooks();
            Container.BindInstance(_catalog).AsSingle();
            Container.BindInstance(_looks).AsSingle();
        }

        private void RequireCatalog()
        {
            if (_catalog == null)
                throw new InvalidOperationException($"Character settings '{name}' have no catalog: assign Assets/Game/Content/Characters/CharacterCatalog.asset.");

            if (_catalog.Characters.Length == 0)
                throw new InvalidOperationException($"The character catalog '{_catalog.name}' is empty: add at least one character, the first one is the default.");

            var keys = new HashSet<int>();
            foreach (var entry in _catalog.Characters)
            {
                if (entry.Character == null)
                    throw new InvalidOperationException(
                        $"The character catalog '{_catalog.name}' has a row without a character: assign a character description from Assets/Game/Content/Characters.");

                if (entry.Character.Key == 0 || !keys.Add(entry.Character.Key))
                    throw new InvalidOperationException(
                        $"Character '{entry.Character.name}' is listed twice or has no key of its own ({entry.Character.Key.ToString()}): the editor issues keys on import, so open the project in the main editor.");

                if (entry.Prefab == null)
                    throw new InvalidOperationException($"Character '{entry.Character.name}' in the catalog '{_catalog.name}' has no prefab.");
            }
        }

        private void RequireLooks()
        {
            if (_looks == null)
                throw new InvalidOperationException($"Character settings '{name}' have no looks: assign Assets/Game/Content/Characters/CharacterLooksConfig.asset.");

            var looked = new HashSet<CharacterDescription>();
            foreach (var look in _looks.Looks)
            {
                if (look.Character == null || !looked.Add(look.Character))
                    throw new InvalidOperationException($"'{_looks.name}' has a row without a character or a character twice: every character needs exactly one look.");

                if (look.Portrait == null || string.IsNullOrWhiteSpace(look.DisplayName))
                    throw new InvalidOperationException($"The look of '{look.Character.name}' has no portrait or no name: the character select and the winner window show both.");
            }

            foreach (var entry in _catalog.Characters)
                if (!looked.Contains(entry.Character))
                    throw new InvalidOperationException($"Character '{entry.Character.name}' has no look in '{_looks.name}': add a row with its portrait and name.");
        }
    }
}

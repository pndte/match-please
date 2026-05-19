using Bw.UseCases.Character.Extensions;
using Cysharp.Threading.Tasks;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character
{
    public class DeadCharactersDestroyer
    {
        public DeadCharactersDestroyer(Lifetime lifetime,
            IGameObjectByCharacterCollection gameObjectByCharacterCollection)
        {
            gameObjectByCharacterCollection.ForEach(lifetime, OnCharacterAdded);
        }

        private void OnCharacterAdded(Lifetime lifetime, ICharacter character, GameObject characterGo)
        {
            character.State.WhenDead(lifetime, async _ =>
            {
                await UniTask.Yield();
                Object.Destroy(characterGo); //TODO: временное решение, потом будем пуллировать
            });
        }
    }
}
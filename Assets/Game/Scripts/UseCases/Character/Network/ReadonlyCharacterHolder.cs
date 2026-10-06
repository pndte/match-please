using Bw.Entities;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Character.Network
{
    public sealed class ReadonlyCharacterHolder : MonoBehaviour, IHolder<IReadonlyCharacter>
    {
        public IReadonlyCharacter Value { get; private set; }

        [Inject]
        private void Construct(IReadonlyCharacter character)
        {
            Value = character;
        }
    }
}

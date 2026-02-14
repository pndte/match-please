using Bw.Entities;
using UnityEngine;
using Zenject;

namespace Bw.UseCases.Character.Network
{
    public class CharacterHolder : MonoBehaviour, IHolder<ICharacter>
    {
        public ICharacter Value => _value;
        private ICharacter _value;

        [Inject]
        public void Construct(ICharacter character)
        {
            _value = character;
        }
    }
}
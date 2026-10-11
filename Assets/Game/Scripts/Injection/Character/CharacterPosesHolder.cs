using Bw.Entities;
using Bw.UseCases.Character.View.Abstractions;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    public sealed class CharacterPosesHolder : MonoBehaviour, IHolder<ICharacterPoses>
    {
        public ICharacterPoses Value { get; private set; }

        [Inject]
        private void Construct(ICharacterPoses poses)
        {
            Value = poses;
        }
    }
}

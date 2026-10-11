using Bw.UseCases.Character.View.Abstractions;
using UnityEngine;

namespace Bw.UseCases.Character.View.Pose
{
    public sealed class CharacterPoses : ICharacterPoses
    {
        private static readonly int PunchState = Animator.StringToHash("Punch");

        private readonly IReadonlyCharacter _character;
        private readonly Animator _animator;

        public CharacterPoses(IReadonlyCharacter character, Animator animator)
        {
            _character = character;
            _animator = animator;
        }

        public void Punch()
        {
            if (_character.State.Value == CharacterState.Alive)
                _animator.Play(PunchState, 0, 0f);
        }
    }
}

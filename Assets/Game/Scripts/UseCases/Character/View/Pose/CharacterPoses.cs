using Bw.UseCases.Character.View.Abstractions;
using Bw.UseCases.Mortality.View.Death;
using UnityEngine;

namespace Bw.UseCases.Character.View.Pose
{
    public sealed class CharacterPoses : ICharacterPoses
    {
        private static readonly int PunchState = Animator.StringToHash("Punch");

        private readonly IDeathViewModel _death;
        private readonly Animator _animator;

        public CharacterPoses(IDeathViewModel death, Animator animator)
        {
            _death = death;
            _animator = animator;
        }

        public void Punch()
        {
            if (!_death.Dead.Value)
                _animator.Play(PunchState, 0, 0f);
        }
    }
}

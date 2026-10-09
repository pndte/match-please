using Bw.UseCases.Character.Extensions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.Death
{
    public sealed class CharacterDeathAnimation
    {
        private static readonly int Death = Animator.StringToHash("Death");

        public CharacterDeathAnimation(Lifetime lifetime, IReadonlyCharacter character, Animator animator)
        {
            character.State.WhenDead(lifetime, _ => animator.Play(Death, 0, 1f));
            character.AdviseKilled(lifetime, () => animator.Play(Death, 0, 0f));
        }
    }
}

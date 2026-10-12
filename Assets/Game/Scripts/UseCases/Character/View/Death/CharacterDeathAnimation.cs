using Bw.UseCases.Mortality;
using Bw.UseCases.Mortality.Extensions;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.Death
{
    public sealed class CharacterDeathAnimation
    {
        private static readonly int Death = Animator.StringToHash("Death");

        public CharacterDeathAnimation(Lifetime lifetime, IReadonlyMortal character, Animator animator)
        {
            character.State.WhenDead(lifetime, _ => animator.Play(Death, 0, 1f));
            character.AdviseKilled(lifetime, () => animator.Play(Death, 0, 0f));
        }
    }
}

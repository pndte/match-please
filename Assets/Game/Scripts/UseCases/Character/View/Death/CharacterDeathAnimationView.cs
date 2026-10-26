using Bw.UseCases.Mortality.View.Death;
using JetBrains.Collections.Viewable;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.Death
{
    public sealed class CharacterDeathAnimationView
    {
        private static readonly int Death = Animator.StringToHash("Death");

        public CharacterDeathAnimationView(Lifetime lifetime, IDeathViewModel death, Animator animator)
        {
            death.Dead.WhenTrue(lifetime, _ => animator.Play(Death, 0, 1f));
            death.Killed.Advise(lifetime, _ => animator.Play(Death, 0, 0f));
        }
    }
}

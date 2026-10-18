using Bw.UseCases.Movement.View.Animation;
using Bw.UseCases.Movement.View.Animation.Abstractions;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    public sealed class SpriteLookInstaller : MonoInstaller
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _sprite;
        [SerializeField] private AnimationFootsteps _footsteps;

        public override void InstallBindings()
        {
            Container.Bind<Animator>().FromInstance(_animator).AsSingle();
            Container.Bind<SpriteRenderer[]>().FromInstance(new[] { _sprite }).AsSingle();
            Container.Bind<IFootsteps>().FromInstance(_footsteps).AsSingle();
            Container.BindInterfacesTo<SpriteMovementAnimator>().AsSingle().WithArguments(_animator, _sprite);
        }
    }
}

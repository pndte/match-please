using Bw.UseCases.Movement.View.Animation;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    public sealed class SpriteLookInstaller : MonoInstaller
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private SpriteRenderer _sprite;

        public override void InstallBindings()
        {
            Container.Bind<Animator>().FromInstance(_animator).AsSingle();
            Container.Bind<SpriteRenderer[]>().FromInstance(new[] { _sprite }).AsSingle();
            Container.BindInterfacesTo<SpriteMovementAnimator>().AsSingle().WithArguments(_animator, _sprite);
        }
    }
}

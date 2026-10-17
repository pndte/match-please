using System;
using Bw.UseCases.Movement.View.Animation;
using UnityEngine;
using Zenject;

namespace Bw.Injection
{
    public sealed class RigLookInstaller : MonoInstaller
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private Transform _rig;
        [SerializeField] private SpriteRenderer[] _sprites;

        public override void InstallBindings()
        {
            if (_sprites.Length == 0)
                throw new InvalidOperationException($"The rig look '{name}' lists no sprites: a hit would flash nothing.");

            Container.Bind<Animator>().FromInstance(_animator).AsSingle();
            Container.Bind<SpriteRenderer[]>().FromInstance(_sprites).AsSingle();
            Container.BindInterfacesTo<RigMovementAnimator>().AsSingle().WithArguments(_animator, _rig);
        }
    }
}

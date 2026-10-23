using Bw.Entities.Simulation;
using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon;
using Bw.UseCases.Combat.Weapon.Abstractions;
using Zenject;

namespace Bw.Injection.Weapon
{
    public sealed class WeaponSimulatorInstaller<TState, TSimulator> : Installer<WeaponSimulatorInstaller<TState, TSimulator>>
        where TState : struct
        where TSimulator : IWeaponSimulator<TState>
    {
        public override void InstallBindings()
        {
            Container.Bind<IWeaponSimulator<TState>>().To<TSimulator>().AsSingle();
            Container.Decorate<IWeaponSimulator<TState>>().With<GuardedWeaponSimulator<TState>>();
            Container.Bind<ISimulator<WeaponInput, TState>>().FromResolveGetter<IWeaponSimulator<TState>>(simulator => simulator).AsSingle();
        }
    }
}

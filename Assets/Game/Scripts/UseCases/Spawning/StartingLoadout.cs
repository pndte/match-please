using Bw.UseCases.Combat.Abstractions.Weapons;
using Bw.UseCases.Combat.Weapon;
using JetBrains.Lifetimes;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Bw.UseCases.Spawning
{
    public sealed class StartingLoadout
    {
        private readonly Lifetime _lifetime;
        private readonly IArmory _armory;
        private readonly WeaponDescription _fists;
        private readonly WeaponDescription[] _weapons;

        public StartingLoadout(
            Lifetime lifetime,
            IArmory armory,
            WeaponDescription fists,
            WeaponDescription[] weapons)
        {
            _lifetime = lifetime;
            _armory = armory;
            _fists = fists;
            _weapons = weapons;
        }

        public IWeapon CreateGun(Vector3 position) =>
            _armory.CreateWeapon(_lifetime, RandomWeapon(), position, Quaternion.identity);

        public IWeapon CreateFists(Lifetime lifetime, Vector3 position) =>
            _armory.CreateWeapon(lifetime, _fists, position, Quaternion.identity);

        private WeaponDescription RandomWeapon() =>
            _weapons[Random.Range(0, _weapons.Length)];
    }
}

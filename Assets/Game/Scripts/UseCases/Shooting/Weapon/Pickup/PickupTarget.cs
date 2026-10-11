using System;
using System.Runtime.CompilerServices;
using Bw.UseCases.Shooting.Weapon.Pickup.Abstractions;

namespace Bw.UseCases.Shooting.Weapon.Pickup
{
    public abstract class PickupTarget : IEquatable<PickupTarget>
    {
        public static readonly PickupTarget Nothing = new NothingTarget();

        private PickupTarget()
        {
        }

        public static PickupTarget Of(IFloorWeapon weapon) =>
            new WeaponTarget(weapon);

        public abstract bool IsWeapon(IFloorWeapon weapon);

        public abstract void Switch(Action nothing, Action<IFloorWeapon> weapon);

        public abstract bool Equals(PickupTarget other);

        public override bool Equals(object other) =>
            other is PickupTarget target && Equals(target);

        public abstract override int GetHashCode();

        private sealed class NothingTarget : PickupTarget
        {
            public override bool IsWeapon(IFloorWeapon weapon) =>
                false;

            public override void Switch(Action nothing, Action<IFloorWeapon> weapon) =>
                nothing();

            public override bool Equals(PickupTarget other) =>
                other is NothingTarget;

            public override int GetHashCode() =>
                0;
        }

        private sealed class WeaponTarget : PickupTarget
        {
            private readonly IFloorWeapon _weapon;

            public WeaponTarget(IFloorWeapon weapon)
            {
                _weapon = weapon;
            }

            public override bool IsWeapon(IFloorWeapon weapon) =>
                weapon == _weapon;

            public override void Switch(Action nothing, Action<IFloorWeapon> weapon) =>
                weapon(_weapon);

            public override bool Equals(PickupTarget other) =>
                other.IsWeapon(_weapon);

            public override int GetHashCode() =>
                RuntimeHelpers.GetHashCode(_weapon);
        }
    }
}

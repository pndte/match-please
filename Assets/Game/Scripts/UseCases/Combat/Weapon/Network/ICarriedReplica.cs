using Bw.UseCases.Combat.Abstractions.Weapons;


namespace Bw.UseCases.Combat.Weapon.Network
{
    public interface ICarriedReplica
    {
        public void DropAhead(IWeapon weapon);
        public void Restore(IWeapon weapon);
    }
}

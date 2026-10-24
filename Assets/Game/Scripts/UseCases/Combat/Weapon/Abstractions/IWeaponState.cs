namespace Bw.UseCases.Combat.Weapon.Abstractions
{
    public interface IWeaponState
    {
        public float Aim { get; }
        public int Shots { get; }
        public bool Stowable { get; }
        public bool Rooting { get; }
    }
}

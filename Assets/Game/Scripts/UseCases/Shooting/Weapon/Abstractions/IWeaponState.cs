namespace Bw.UseCases.Shooting.Weapon.Abstractions
{
    public interface IWeaponState
    {
        public float Aim { get; }
        public int Shots { get; }
        public bool Stowable { get; }
    }
}

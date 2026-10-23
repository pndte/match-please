using Bw.UseCases.Combat.Abstractions.Weapons;


namespace Bw.UseCases.Combat.Hands
{
    public readonly struct HandsInput
    {
        public readonly WeaponInput Weapon;
        public readonly DrawChoice Choice;

        public HandsInput(WeaponInput weapon, DrawChoice choice)
        {
            Weapon = weapon;
            Choice = choice;
        }
    }
}

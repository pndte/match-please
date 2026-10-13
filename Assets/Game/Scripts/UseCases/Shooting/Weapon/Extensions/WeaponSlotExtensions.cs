using Bw.UseCases.Shooting.Weapon.Abstractions;

namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class WeaponSlotExtensions
    {
        public static bool IsFor(this WeaponSlot slot, WeaponKind kind)
        {
            foreach (var accepted in slot.Kinds)
                if (accepted == kind)
                    return true;

            return false;
        }
    }
}

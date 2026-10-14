using System.Linq;
using JetBrains.Lifetimes;
using UnityEngine.UIElements;

namespace Setup.Launch.Menu
{
    public static class PortFields
    {
        private const int MaxLength = 5;

        public static void AdviseDigitsOnly(this TextField field, Lifetime lifetime) =>
            field.AdviseChanged(lifetime, text =>
            {
                var digits = new string(text.Where(char.IsDigit).Take(MaxLength).ToArray());
                if (digits != text)
                    field.SetValueWithoutNotify(digits);

                field.EnableInClassList(MenuClasses.FieldError, !TryParse(digits, out _));
            });

        public static bool TryParse(string text, out ushort port) =>
            ushort.TryParse(text, out port) && port > 0;
    }
}

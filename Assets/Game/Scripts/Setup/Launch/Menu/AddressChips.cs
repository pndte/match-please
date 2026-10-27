using UnityEngine;
using UnityEngine.UIElements;

namespace Setup.Launch.Menu
{
    public static class AddressChips
    {
        public static void Fill(VisualElement container)
        {
            container.Clear();
            var addresses = LocalAddresses.Read();
            if (addresses.Length == 0)
            {
                var none = new Label("адрес не определился — посмотри его в настройках сети");
                none.AddToClassList("hint");
                container.Add(none);
                return;
            }

            foreach (var address in addresses)
                container.Add(Chip(address));
        }

        private static VisualElement Chip(string address)
        {
            var chip = new VisualElement();
            chip.AddToClassList("chip");
            var text = new Label(address);
            text.AddToClassList("chip__text");
            chip.Add(text);
            var copy = new Button(() => GUIUtility.systemCopyBuffer = address);
            copy.AddToClassList("chip__copy");
            chip.Add(copy);
            return chip;
        }
    }
}

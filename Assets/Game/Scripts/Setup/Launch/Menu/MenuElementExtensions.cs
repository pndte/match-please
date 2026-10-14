using System;
using JetBrains.Lifetimes;
using UnityEngine.UIElements;

namespace Setup.Launch.Menu
{
    public static class MenuElementExtensions
    {
        public static void AdviseClicked(this Button button, Lifetime lifetime, Action handler) =>
            lifetime.Bracket(() => button.clicked += handler, () => button.clicked -= handler);

        public static void AdviseChanged(this TextField field, Lifetime lifetime, Action<string> handler)
        {
            EventCallback<ChangeEvent<string>> changed = change => handler(change.newValue);
            lifetime.Bracket(() => field.RegisterValueChangedCallback(changed), () => field.UnregisterValueChangedCallback(changed));
        }
    }
}

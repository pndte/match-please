namespace Bw.UseCases.Combat.View.Rig
{
    public static class WeaponRigParameters
    {
        public const string Held = "Held";
        public const string Reloading = "Reloading";
        public const string Empty = "Empty";
        public const string ReloadProgress = "ReloadProgress";
        //TODO: удар пушкой на ПКМ не запрограммирован: в WeaponRig.controller есть триггер Punch и PunchVariant (0..2) для трёх ударов кулаком вместо пушки, их пока никто не выставляет
    }
}

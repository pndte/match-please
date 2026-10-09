namespace Bw.UseCases.Shooting.View.Rig
{
    public static class WeaponRigParameters
    {
        public const string Held = "Held";
        public const string Reloading = "Reloading";
        public const string Empty = "Empty";
        public const string ReloadProgress = "ReloadProgress";
        //TODO: удар на ПКМ не запрограммирован: в WeaponRig.controller уже есть триггер Punch и PunchVariant (0..2) для трёх ударов кулаком, у персонажей есть состояние Punch той же длины (0.28 с)
    }
}

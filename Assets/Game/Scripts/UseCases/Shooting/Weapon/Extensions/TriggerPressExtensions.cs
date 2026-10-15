using System;

namespace Bw.UseCases.Shooting.Weapon.Extensions
{
    public static class TriggerPressExtensions
    {
        public static TriggerPress WithAge(this TriggerPress press, int age) =>
            new(press.Number, ByteOf(age), press.ViewDelay, press.PredictedDelay, press.Late);

        public static TriggerPress Delivered(this TriggerPress press, int lateTicks) =>
            new(press.Number, 0, press.ViewDelay, press.PredictedDelay, ByteOf(lateTicks));

        private static byte ByteOf(int ticks) =>
            (byte)Math.Clamp(ticks, 0, byte.MaxValue);
    }
}

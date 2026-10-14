using System;

namespace Bw.UseCases.Shooting.View.Abstractions
{
    public readonly struct GunReload : IEquatable<GunReload>
    {
        public static readonly GunReload None = default;

        public readonly bool Active;
        public readonly bool Empty;
        public readonly float Progress;
        public readonly float Seconds;
        public readonly float SecondsLeft;

        public GunReload(
            bool empty,
            float progress,
            float seconds,
            float secondsLeft)
        {
            Active = true;
            Empty = empty;
            Progress = progress;
            Seconds = seconds;
            SecondsLeft = secondsLeft;
        }

        public bool Equals(GunReload other) =>
            Active == other.Active && Empty == other.Empty && Progress == other.Progress && Seconds == other.Seconds && SecondsLeft == other.SecondsLeft;
    }
}

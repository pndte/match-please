using System;

namespace Bw.UseCases.Level.Extensions
{
    public static class MovingPlatformPathExtensions
    {
        public static float AlongAt(this MovingPlatformPath path, double seconds) =>
            (float)Progress(path, Phase(path, seconds));

        private static double Phase(MovingPlatformPath path, double seconds)
        {
            var cycle = 2d * (path.TravelSeconds + path.PauseSeconds);
            return seconds - Math.Floor(seconds / cycle) * cycle;
        }

        private static double Progress(MovingPlatformPath path, double phase)
        {
            if (phase < path.PauseSeconds)
                return 0d;

            var away = phase - path.PauseSeconds;
            if (away < path.TravelSeconds)
                return Eased(away / path.TravelSeconds);

            var parked = away - path.TravelSeconds;
            if (parked < path.PauseSeconds)
                return 1d;

            return 1d - Eased((parked - path.PauseSeconds) / path.TravelSeconds);
        }

        private static double Eased(double share) =>
            (1d - Math.Cos(Math.PI * share)) / 2d;
    }
}

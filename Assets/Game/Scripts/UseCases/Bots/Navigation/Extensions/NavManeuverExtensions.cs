using Bw.UseCases.Movement;

namespace Bw.UseCases.Bots.Navigation.Extensions
{
    public static class NavManeuverExtensions
    {
        public static bool IsRun(this NavManeuver maneuver) =>
            maneuver.JumpTicks == 0 && !maneuver.Dropping;

        public static MovementInput InputAt(this NavManeuver maneuver, int tick) =>
            new(tick >= maneuver.RunDelay ? maneuver.Horizontal : 0f, maneuver.JumpTicks > 0 && tick == 0, tick < maneuver.JumpTicks, maneuver.Dropping);
    }
}

using System;

namespace Bw.UseCases.Bots.Goap
{
    [Flags]
    public enum BotFact
    {
        None = 0,
        TargetNoticed = 1 << 0,
        TargetInSight = 1 << 1,
        TargetClose = 1 << 2,
        TargetInReach = 1 << 3,
        TargetDown = 1 << 4,
        GunDrawn = 1 << 5,
        FistsDrawn = 1 << 6,
        PropHeld = 1 << 7,
        PropNearby = 1 << 8,
        Hurt = 1 << 9,
        Roamed = 1 << 10
    }
}

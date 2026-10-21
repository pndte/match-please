using System;

namespace Bw.UseCases.Bots.Navigation
{
    [Serializable]
    public struct NavManeuver
    {
        public float Horizontal;
        public int JumpTicks;
        public int RunDelay;
        public bool Dropping;
    }
}

using System;

namespace Bw.UseCases.Bots.Navigation
{
    [Serializable]
    public struct NavEdge
    {
        public int From;
        public int To;
        public int Ticks;
        public NavManeuver Maneuver;
    }
}

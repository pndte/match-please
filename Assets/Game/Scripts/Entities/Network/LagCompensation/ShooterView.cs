using System;

namespace Bw.Entities.Network.LagCompensation
{
    public readonly struct ShooterView
    {
        public readonly double Interpolated;
        public readonly double Predicted;

        private readonly PlayerDriver _shooter;

        public ShooterView(double interpolated, double predicted, PlayerDriver shooter)
        {
            Interpolated = interpolated;
            Predicted = predicted;
            _shooter = shooter;
        }

        public double TickOf(IBubbleHistory bubble) =>
            _shooter.Switch(
                (Bubble: bubble, View: this),
                static (state, client) => state.Bubble.Inside(client, state.View.Predicted) ? state.View.Predicted : state.View.Interpolated,
                static state => state.View.Interpolated);

        public ShooterView Clamped(double earliest, double latest) =>
            new(Math.Clamp(Interpolated, earliest, latest), Math.Clamp(Predicted, earliest, latest), _shooter);
    }
}

using System;
using Bw.Entities.Network.Prediction.Bubble;
using UnityEngine;

namespace Bw.Entities.Network.LagCompensation
{
    public readonly struct ShooterView
    {
        public readonly double Interpolated;
        public readonly double Predicted;

        private readonly Vector2 _center;
        private readonly PredictionBubbleConfig _bubble;

        public ShooterView(
            double interpolated,
            double predicted,
            Vector2 center,
            PredictionBubbleConfig bubble)
        {
            Interpolated = interpolated;
            Predicted = predicted;
            _center = center;
            _bubble = bubble;
        }

        public double TickOf(Vector2 position) =>
            _bubble.Covers(_center, position) ? Predicted : Interpolated;

        public ShooterView Clamped(double earliest, double latest) =>
            new(Math.Clamp(Interpolated, earliest, latest), Math.Clamp(Predicted, earliest, latest), _center, _bubble);
    }
}

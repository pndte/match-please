using System;

namespace Bw.UseCases.Shooting.Weapon
{
    public readonly struct TriggerPress : IEquatable<TriggerPress>
    {
        public readonly byte Number;
        public readonly byte Age;
        public readonly float ViewDelay;
        public readonly float PredictedDelay;
        public readonly byte Late;

        public TriggerPress(
            byte number,
            byte age,
            float viewDelay,
            float predictedDelay,
            byte late)
        {
            Number = number;
            Age = age;
            ViewDelay = viewDelay;
            PredictedDelay = predictedDelay;
            Late = late;
        }

        public bool Equals(TriggerPress other) =>
            Number == other.Number
            && Age == other.Age
            && ViewDelay.Equals(other.ViewDelay)
            && PredictedDelay.Equals(other.PredictedDelay)
            && Late == other.Late;
    }
}

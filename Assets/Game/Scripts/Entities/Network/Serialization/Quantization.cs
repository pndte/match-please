using System;

namespace Bw.Entities.Network.Serialization
{
    public readonly struct Quantization
    {
        private readonly double _stepsPerUnit;

        public Quantization(int fractionBits)
        {
            _stepsPerUnit = Math.Pow(2d, fractionBits);
        }

        public int StepsOf(float value) =>
            checked((int)Math.Round(value * _stepsPerUnit));

        public long StepsOf(double value) =>
            checked((long)Math.Round(value * _stepsPerUnit));

        public float ValueOf(int steps) =>
            (float)(steps / _stepsPerUnit);

        public double ValueOf(long steps) =>
            steps / _stepsPerUnit;
    }
}

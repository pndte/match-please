using Bw.Entities.Simulation;
using Bw.UseCases.Bots.Abstractions;
using Bw.UseCases.Bots.Extensions;
using Bw.UseCases.Bots.Senses;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Bw.UseCases.Bots
{
    public sealed class BotGunner //TODO: бот целится в центр цели и не берёт упреждение; дальность оружия он не знает и стреляет, как только видит
    {
        private const float SeedRange = 1000f;

        private readonly IBotGamepad _gamepad;
        private readonly BotDifficultyConfig _config;
        private readonly ISimulationStep _step;

        private int _ticks;
        private float _seed;

        public BotGunner(IBotGamepad gamepad, BotDifficultyConfig config, ISimulationStep step)
        {
            _gamepad = gamepad;
            _config = config;
            _step = step;
        }

        public void Begin()
        {
            _ticks = 0;
            _seed = Random.value * SeedRange;
        }

        public void FireAt(Vector2 eye, Sighting target)
        {
            var time = _ticks++ * _step.Duration;
            var error = (Mathf.PerlinNoise(_seed, time * _config.AimWander) * 2f - 1f) * _config.AimError;
            _gamepad.AimAt(eye, target.Position, error);
            if (target.Visible && IsBursting(time))
                _gamepad.PullTrigger();
        }

        private bool IsBursting(float time) =>
            time % (_config.BurstTime + _config.PauseTime) < _config.BurstTime;
    }
}

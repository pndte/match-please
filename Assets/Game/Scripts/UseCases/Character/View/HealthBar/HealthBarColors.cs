using Bw.UseCases.Character.View.Player;
using JetBrains.Lifetimes;
using UnityEngine;

namespace Bw.UseCases.Character.View.HealthBar
{
    public sealed class HealthBarColors
    {
        private static readonly int FillColorId = Shader.PropertyToID("_FillColor");
        private static readonly int FillShadeId = Shader.PropertyToID("_FillShade");

        private readonly Material _material;
        private readonly HealthBarConfig _config;

        public HealthBarColors(
            Lifetime lifetime,
            Material material,
            IPlayerViewModel player,
            HealthBarConfig config)
        {
            _material = material;
            _config = config;

            player.Mine.Advise(lifetime, Paint);
        }

        private void Paint(bool mine)
        {
            _material.SetColor(FillColorId, mine ? _config.OwnFill : _config.OtherFill);
            _material.SetColor(FillShadeId, mine ? _config.OwnShade : _config.OtherShade);
        }
    }
}

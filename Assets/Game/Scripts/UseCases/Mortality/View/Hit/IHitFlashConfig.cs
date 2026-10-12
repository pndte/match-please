using UnityEngine;

namespace Bw.UseCases.Mortality.View.Hit
{
    public interface IHitFlashConfig
    {
        public Color FlashColor { get; }
        public float FlashStrength { get; }
        public float FlashDuration { get; }
    }
}

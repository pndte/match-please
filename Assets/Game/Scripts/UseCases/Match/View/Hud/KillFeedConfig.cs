using UnityEngine;
using UnityEngine.UIElements;

namespace Bw.UseCases.Match.View.Hud
{
    [CreateAssetMenu(fileName = "KillFeedConfig", menuName = "Configs/KillFeedConfig")]
    public sealed class KillFeedConfig : ScriptableObject
    {
        public VisualTreeAsset Tile;
        [Min(1)] public int Limit = 5;
        [Min(0)] public float ShownSeconds = 5f;
        [Min(0)] public float FadeSeconds = 0.6f; //TODO: время таяния повторяет transition-duration у .kill--leaving в Hud.uss — брать его из стиля, а не держать вторую копию
    }
}

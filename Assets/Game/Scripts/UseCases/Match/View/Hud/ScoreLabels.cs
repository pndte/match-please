using System.Globalization;
using Bw.UseCases.Match.Extensions;
using UnityEngine.UIElements;

namespace Bw.UseCases.Match.View.Hud
{
    public static class ScoreLabels
    {
        private const string NoDamagePerKill = "—";

        public static void Fill(VisualElement element, Score score)
        {
            element.Q<Label>("kills").text = score.Kills.ToString(CultureInfo.InvariantCulture);
            element.Q<Label>("damage").text = score.Damage.ToString(CultureInfo.InvariantCulture);
            element.Q<Label>("deaths").text = score.Deaths.ToString(CultureInfo.InvariantCulture);
            element.Q<Label>("kd").text = score.KillsPerDeath().ToString("0.00", CultureInfo.InvariantCulture);
            element.Q<Label>("damage-per-kill").text = score.TryGetDamagePerKill(out var damagePerKill)
                ? damagePerKill.ToString(CultureInfo.InvariantCulture)
                : NoDamagePerKill;
        }
    }
}

using System.Globalization;
using UnityEngine.UIElements;

namespace Bw.UseCases.Match.View.Hud
{
    public static class ScoreLabels
    {
        private const string NoDamagePerKill = "—";

        public static void Fill(VisualElement element, ScoreStats stats)
        {
            element.Q<Label>("kills").text = stats.Kills.ToString(CultureInfo.InvariantCulture);
            element.Q<Label>("damage").text = stats.Damage.ToString(CultureInfo.InvariantCulture);
            element.Q<Label>("deaths").text = stats.Deaths.ToString(CultureInfo.InvariantCulture);
            element.Q<Label>("kd").text = stats.KillsPerDeath.ToString("0.00", CultureInfo.InvariantCulture);
            element.Q<Label>("damage-per-kill").text = stats.Kills > 0
                ? stats.DamagePerKill.ToString(CultureInfo.InvariantCulture)
                : NoDamagePerKill;
        }
    }
}

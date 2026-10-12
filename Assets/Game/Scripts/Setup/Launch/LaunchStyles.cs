using UnityEngine;

namespace Setup.Launch
{
    public sealed class LaunchStyles
    {
        public GUIStyle Title { get; } = new(GUI.skin.label) { fontSize = 44, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        public GUIStyle Text { get; } = new(GUI.skin.label) { fontSize = 22, wordWrap = true };
        public GUIStyle Hint { get; } = new(GUI.skin.label) { fontSize = 18, wordWrap = true, normal = { textColor = new Color(0.75f, 0.75f, 0.75f) } };
        public GUIStyle Error { get; } = new(GUI.skin.label) { fontSize = 18, wordWrap = true, normal = { textColor = new Color(1f, 0.45f, 0.4f) } };
        public GUIStyle Field { get; } = new(GUI.skin.textField) { fontSize = 26, fixedHeight = 44f, alignment = TextAnchor.MiddleLeft };
        public GUIStyle Button { get; } = new(GUI.skin.button) { fontSize = 24, fixedHeight = 52f };
    }
}

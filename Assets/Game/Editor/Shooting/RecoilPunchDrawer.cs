using Bw.UseCases.Shooting.View.Recoil;
using UnityEditor;
using UnityEngine;

namespace Bw.EditorTools.Shooting
{
    [CustomPropertyDrawer(typeof(RecoilPunch))]
    public sealed class RecoilPunchDrawer : PropertyDrawer
    {
        private const int SparklinePoints = 64;
        private const float SparklineTail = 1.15f;
        private const float FieldLabelWidth = 64f;
        private const float TimeLabelWidth = 32f;
        private const float TimeWidth = 84f;
        private const float OvershootLabelWidth = 62f;
        private const float OvershootWidth = 104f;
        private const float Gap = 6f;

        private static readonly Color SparklineBackground = new(0f, 0f, 0f, 0.18f);
        private static readonly Color SparklineCurve = new(0.98f, 0.62f, 0.25f);
        private static readonly Color SparklineRest = new(1f, 1f, 1f, 0.15f);
        private static readonly Vector3[] Points = new Vector3[SparklinePoints];
        private static readonly GUIContent AmountLabel = new("Amount");
        private static readonly GUIContent OutLabel = new("Out", "Seconds from the shot to the full amount.");
        private static readonly GUIContent ReturnLabel = new("Back", "Seconds from the full amount back to rest.");
        private static readonly GUIContent OvershootLabel = new("Overshoot", "How far it swings past rest on the way back: the overshoot of the OutBack ease, 0 is none.");

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUIUtility.singleLineHeight * 2f + EditorGUIUtility.standardVerticalSpacing;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var amount = property.FindPropertyRelative(nameof(RecoilPunch.Amount));
            var outTime = property.FindPropertyRelative(nameof(RecoilPunch.OutTime));
            var returnTime = property.FindPropertyRelative(nameof(RecoilPunch.ReturnTime));
            var overshoot = property.FindPropertyRelative(nameof(RecoilPunch.Overshoot));

            EditorGUI.BeginProperty(position, label, property);
            var line = EditorGUIUtility.singleLineHeight;
            var fields = EditorGUI.PrefixLabel(new Rect(position.x, position.y, position.width, line), label);
            var labelWidth = EditorGUIUtility.labelWidth;
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth = FieldLabelWidth;

            EditorGUI.PropertyField(new Rect(fields.x, fields.y, fields.width, line), amount, AmountLabel);

            var second = new Rect(fields.x, position.y + line + EditorGUIUtility.standardVerticalSpacing, fields.width, line);
            var timeWidth = Mathf.Min(TimeWidth, second.width * 0.22f);
            var overshootWidth = Mathf.Min(OvershootWidth, second.width * 0.3f);
            EditorGUIUtility.labelWidth = TimeLabelWidth;
            EditorGUI.PropertyField(new Rect(second.x, second.y, timeWidth, line), outTime, OutLabel);
            EditorGUI.PropertyField(new Rect(second.x + timeWidth + Gap, second.y, timeWidth, line), returnTime, ReturnLabel);
            EditorGUIUtility.labelWidth = OvershootLabelWidth;
            EditorGUI.PropertyField(new Rect(second.x + (timeWidth + Gap) * 2f, second.y, overshootWidth, line), overshoot, OvershootLabel);

            var sparklineX = second.x + (timeWidth + Gap) * 2f + overshootWidth + Gap;
            var sparkline = new Rect(sparklineX, second.y + 1f, second.xMax - sparklineX, line - 2f);
            if (Event.current.type == EventType.Repaint)
                DrawSparkline(sparkline, new RecoilPunch { Amount = 1f, OutTime = outTime.floatValue, ReturnTime = returnTime.floatValue, Overshoot = overshoot.floatValue });

            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        private static void DrawSparkline(Rect rect, RecoilPunch punch)
        {
            EditorGUI.DrawRect(rect, SparklineBackground);
            if (rect.width < 8f || punch.OutTime <= 0f || punch.ReturnTime <= 0f)
                return;

            var seconds = punch.Duration() * SparklineTail;
            for (var index = 0; index < SparklinePoints; index++)
            {
                var offset = punch.OffsetAt(seconds * index / (SparklinePoints - 1));
                Points[index] = new Vector3(
                    rect.x + rect.width * index / (SparklinePoints - 1),
                    rect.yMax - 2f - (rect.height - 4f) * Mathf.Clamp(offset, -0.25f, 1f) / 1.25f - (rect.height - 4f) * 0.2f);
            }

            var rest = rect.yMax - 2f - (rect.height - 4f) * 0.2f;
            Handles.color = SparklineRest;
            Handles.DrawLine(new Vector3(rect.x, rest), new Vector3(rect.xMax, rest));
            Handles.color = SparklineCurve;
            Handles.DrawAAPolyLine(2f, Points);
        }
    }
}

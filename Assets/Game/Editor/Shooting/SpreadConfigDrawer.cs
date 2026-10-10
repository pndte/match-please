using Bw.UseCases.Shooting.Weapon;
using UnityEditor;
using UnityEngine;

namespace Bw.EditorTools.Shooting
{
    [CustomPropertyDrawer(typeof(SpreadConfig))]
    public sealed class SpreadConfigDrawer : PropertyDrawer
    {
        private const float FieldLabelWidth = 70f;
        private const float Gap = 6f;

        private static readonly GUIContent ClimbLabel = new("Climb °", "How far every shot throws the cursor up around the shooter. The real mouse cursor moves, so it stays where the shot threw it: the player brings the aim back.");
        private static readonly GUIContent JitterLabel = new("Jitter ±°", "A random throw of the cursor up or down added to the climb of every shot.");

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) =>
            EditorGUIUtility.singleLineHeight;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);
            var fields = EditorGUI.PrefixLabel(position, label);
            var labelWidth = EditorGUIUtility.labelWidth;
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUIUtility.labelWidth = FieldLabelWidth;

            var half = (fields.width - Gap) * 0.5f;
            Field(new Rect(fields.x, fields.y, half, fields.height), property, nameof(SpreadConfig.Climb), ClimbLabel);
            Field(new Rect(fields.x + half + Gap, fields.y, half, fields.height), property, nameof(SpreadConfig.Jitter), JitterLabel);

            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        private static void Field(Rect rect, SerializedProperty property, string name, GUIContent label) =>
            EditorGUI.PropertyField(rect, property.FindPropertyRelative(name), label);
    }
}

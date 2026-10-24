using Bw.UseCases.Combat.Shooting;
using UnityEditor;
using UnityEngine;

namespace Bw.EditorTools.Shooting
{
    [CustomPropertyDrawer(typeof(SpreadConfig))]
    public sealed class SpreadConfigDrawer : PropertyDrawer
    {
        private const float FieldLabelWidth = 70f;
        private const float Gap = 6f;

        private static readonly GUIContent ClimbLabel = new("Climb °", "How far every shot throws the barrel up off the cursor. The cursor stays where the player points.");
        private static readonly GUIContent JitterLabel = new("Jitter ±°", "A random throw of the barrel up or down added to the climb of every shot.");
        private static readonly GUIContent ReturnLabel = new("Return °/s", "How fast the barrel comes back to the cursor, degrees a second, also while firing.");

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

            var third = (fields.width - Gap * 2f) / 3f;
            Field(new Rect(fields.x, fields.y, third, fields.height), property, nameof(SpreadConfig.Climb), ClimbLabel);
            Field(new Rect(fields.x + third + Gap, fields.y, third, fields.height), property, nameof(SpreadConfig.Jitter), JitterLabel);
            Field(new Rect(fields.x + (third + Gap) * 2f, fields.y, third, fields.height), property, nameof(SpreadConfig.Return), ReturnLabel);

            EditorGUIUtility.labelWidth = labelWidth;
            EditorGUI.indentLevel = indent;
            EditorGUI.EndProperty();
        }

        private static void Field(
            Rect rect,
            SerializedProperty property,
            string name,
            GUIContent label) =>
            EditorGUI.PropertyField(rect, property.FindPropertyRelative(name), label);
    }
}

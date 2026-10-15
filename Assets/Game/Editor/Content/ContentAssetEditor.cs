using Bw.Entities.Content;
using UnityEditor;

namespace Bw.EditorTools.Content
{
    [CustomEditor(typeof(ContentAsset), true)]
    public sealed class ContentAssetEditor : Editor
    {
        private static readonly string[] Excluded = { "m_Script", ContentAsset.KeyField };

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            using (new EditorGUI.DisabledScope(true))
                EditorGUILayout.PropertyField(serializedObject.FindProperty(ContentAsset.KeyField));

            DrawPropertiesExcluding(serializedObject, Excluded);
            serializedObject.ApplyModifiedProperties();
        }
    }
}

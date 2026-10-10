using System.Collections.Generic;
using UnityEditor;

namespace Bw.EditorTools.Shooting
{
    public abstract class WeaponFeelEditor : Editor
    {
        private WeaponFeelPreview _preview;
        private IReadOnlyList<LinkedConfigSection> _linked;

        public override bool RequiresConstantRepaint() =>
            _preview.Playing;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawPropertiesExcluding(serializedObject, "m_Script");
            serializedObject.ApplyModifiedProperties();

            foreach (var section in _linked)
                section.Draw();

            _preview.Draw();
        }

        protected abstract PreviewWeapon PreviewedWeapon();

        protected abstract IReadOnlyList<LinkedConfigSection> LinkedSections(PreviewWeapon weapon);

        protected virtual void OnEnable()
        {
            var weapon = PreviewedWeapon();
            _linked = LinkedSections(weapon);
            _preview = new WeaponFeelPreview(weapon, PreviewStep.FromNetwork(), Repaint);
        }

        protected virtual void OnDisable()
        {
            _preview.Dispose();
            foreach (var section in _linked)
                section.Dispose();
        }
    }
}

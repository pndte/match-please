using System;
using System.Collections.Generic;
using System.Linq;
using Bw.Injection.Level;
using Bw.UseCases.Level.View.Lighting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Bw.EditorTools.Level
{
    public sealed class LampScene
    {
        private const string NormalMapName = "_NormalMap";

        private readonly GameObject[] _roots;
        private readonly LampLight _light;
        private readonly LampHaze _haze;

        private LampScene(GameObject[] roots, LampLight light, LampHaze haze)
        {
            _roots = roots;
            _light = light;
            _haze = haze;
        }

        public string Name => _light.gameObject.scene.name;

        public IReadOnlyList<Light2D> Lamps => LampsOf(_light).Union(LampsOf(_haze)).ToList();

        public GameObject LightRoot => BakedOf(_light);

        public GameObject HazeRoot => BakedOf(_haze);

        public IReadOnlyList<SpriteRenderer> Chunks => All<SpriteRenderer>().Where(IsNormalMapped).ToList();

        public Light2D Ambient
        {
            get
            {
                var globals = All<Light2D>().Where(light => light.lightType == Light2D.LightType.Global).ToList();
                if (globals.Count != 1)
                    throw new InvalidOperationException($"'{Name}' has {globals.Count} global 2D lights: the relief is baked over its one ambient.");

                return globals[0];
            }
        }

        public IReadOnlyList<Renderer> Renderers => All<Renderer>().ToList();

        public IReadOnlyList<Light2D> Lights => All<Light2D>().ToList();

        public IReadOnlyList<ShadowCaster2D> Casters => All<ShadowCaster2D>().ToList();

        public IReadOnlyList<ShadowCaster2D> MovingCasters =>
            All<MovingPlatformInstaller>().SelectMany(mover => mover.GetComponentsInChildren<ShadowCaster2D>(true)).ToList();

        public static LampScene Of(Scene scene)
        {
            var roots = scene.GetRootGameObjects();
            return new LampScene(roots, Single<LampLight>(scene, roots), Single<LampHaze>(scene, roots));
        }

        public void ShowLamps(bool lit, bool hazy)
        {
            foreach (var lamp in Lamps)
            {
                lamp.enabled = lit;
                lamp.volumetricEnabled = hazy;
                lamp.volumetricShadowsEnabled = hazy;
            }
        }

        private IEnumerable<T> All<T>() where T : Component =>
            _roots.SelectMany(root => root.GetComponentsInChildren<T>(true));

        private static T Single<T>(Scene scene, GameObject[] roots) where T : Component
        {
            var found = roots.SelectMany(root => root.GetComponentsInChildren<T>(true)).ToList();
            if (found.Count != 1)
                throw new InvalidOperationException($"Scene '{scene.name}' has {found.Count} {typeof(T).Name}: the lamp bake needs exactly one.");

            return found[0];
        }

        private static IEnumerable<Light2D> LampsOf(Component component)
        {
            var lamps = Property(component, "_lamps");
            return Enumerable.Range(0, lamps.arraySize).Select(i => (Light2D)lamps.GetArrayElementAtIndex(i).objectReferenceValue);
        }

        private static GameObject BakedOf(Component component) =>
            Property(component, "_baked").objectReferenceValue as GameObject
            ?? throw new InvalidOperationException($"{component.GetType().Name} of '{component.gameObject.scene.name}' has no baked object: the lamp bake rebuilds its children.");

        private static SerializedProperty Property(Component component, string name) =>
            new SerializedObject(component).FindProperty(name) ?? throw new InvalidOperationException($"{component.GetType().Name} has no serialized '{name}'.");

        private static bool IsNormalMapped(SpriteRenderer renderer)
        {
            if (renderer.sprite == null)
                return false;

            var textures = new SecondarySpriteTexture[renderer.sprite.GetSecondaryTextureCount()];
            renderer.sprite.GetSecondaryTextures(textures);
            return textures.Any(texture => texture.name == NormalMapName);
        }
    }
}

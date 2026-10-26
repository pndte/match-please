using System;
using System.Linq;
using Bw.Entities.Extensions;
using JetBrains.Lifetimes;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace Setup.Launch.Menu
{
    public sealed class MenuDocument
    {
        private const string LayerClass = "layer";
        private const float CameraDepth = -100f;

        public Lifetime Lifetime { get; }

        private readonly VisualElement _root;
        private readonly Camera _camera;
        private readonly SequentialLifetimes _shown;

        private MenuDocument(Lifetime lifetime, VisualElement root, Camera camera)
        {
            Lifetime = lifetime;
            _root = root;
            _camera = camera;
            _shown = new SequentialLifetimes(lifetime);
        }

        public static MenuDocument Create(PanelSettings panel, VisualTreeAsset screen)
        {
            var host = new GameObject("StartMenu");
            host.SetActive(false);
            var document = host.AddComponent<UIDocument>();
            document.panelSettings = panel;
            document.visualTreeAsset = screen;
            var camera = host.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.95f, 0.93f, 0.87f);
            camera.cullingMask = 0;
            camera.depth = CameraDepth;
            camera.enabled = false;
            host.SetActive(true);
            document.rootVisualElement.style.display = DisplayStyle.None;
            return new MenuDocument(host.Lifetime(), document.rootVisualElement, camera);
        }

        public VisualElement Layer(string name) =>
            _root.Q(name) ?? throw new InvalidOperationException($"The start menu has no layer '{name}': StartScreen.uxml holds every screen as a layer.");

        public void Show(IMenuScreen screen)
        {
            var shown = _shown.Next();
            _root.Query(className: LayerClass).ForEach(layer => layer.EnableInClassList(MenuClasses.Hidden, !screen.Layers.Contains(layer.name)));
            _root.style.display = DisplayStyle.Flex;
            _camera.enabled = true;
            EventCallback<KeyDownEvent> keys = key => Route(screen, key);
            shown.Bracket(
                () => _root.RegisterCallback(keys, TrickleDown.TrickleDown),
                () => _root.UnregisterCallback(keys, TrickleDown.TrickleDown));
        }

        public void Hide()
        {
            _shown.TerminateCurrent();
            _root.style.display = DisplayStyle.None;
            _camera.enabled = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ReadInputFromInputManager() =>
            UIToolkitInputConfiguration.SetRuntimeInputBackend(UIToolkitInputBackendOption.LegacyBackend);

        private static void Route(IMenuScreen screen, KeyDownEvent key)
        {
            switch (key.keyCode)
            {
                case KeyCode.Escape:
                    screen.Back();
                    break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    screen.Confirm();
                    break;
                default:
                    return;
            }

            key.StopPropagation();
        }
    }
}

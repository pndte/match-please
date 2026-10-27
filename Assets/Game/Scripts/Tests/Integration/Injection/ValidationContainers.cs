using System;
using System.Collections.Generic;
using System.Linq;
using Bw.Entities.Network;
using Bw.Entities.Pool.GameObjects;
using Bw.Injection;
using Bw.Injection.Prefabs;
using Bw.UseCases;
using Bw.UseCases.Spawning.Network;
using JetBrains.Lifetimes;
using Setup;
using Setup.Launch;
using Unity.Netcode;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;
using Object = UnityEngine.Object;

namespace Bw.Tests.Integration.Injection
{
    public sealed class ValidationContainers : IDisposable
    {
        public const string ProjectContextPrefab = "Assets/Resources/ProjectContext.prefab";
        public const string SetupScene = "Assets/Game/Scenes/GameSetupScene.unity";
        public const string BootScene = "Assets/Game/Scenes/Boot.unity";
        public const string NetworkScene = "Assets/Game/Scenes/Network.unity";
        public const string NetworkManagerPrefab = "Assets/Game/Prefabs/Network/Network.prefab";
        public const string ScenesFolder = "Assets/Game/Scenes";
        private const string ValidationLevel = "Validation";

        private readonly List<Scene> _scenes = new();
        private readonly GameObject _holder;

        private GameObject _networkManager;

        public ValidationContainers()
        {
            _holder = EditorUtility.CreateGameObjectWithHideFlags("Validation", HideFlags.HideAndDontSave);
        }

        public static IEnumerable<string> Scenes() =>
            AssetDatabase.FindAssets("t:Scene", new[] { ScenesFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path != BootScene);

        public static IEnumerable<string> Levels() =>
            AssetDatabase.FindAssets("t:Scene", new[] { ScenesFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path != SetupScene && path != NetworkScene && path != BootScene);

        public static IReadOnlyList<ScriptableObjectInstaller> SettingsInstallers()
        {
            var entries = new List<AddressableAssetEntry>();
            AddressableAssetSettingsDefaultObject.Settings.GetAllAssets(entries, false, null, entry => entry.labels.Contains(AddressableSettingsInstaller.Label));
            return entries
                .Select(entry => AssetDatabase.LoadAssetAtPath<ScriptableObjectInstaller>(entry.AssetPath))
                .OrderBy(installer => installer.name)
                .ToList();
        }

        public static Action<DiContainer> SessionOf(PeerType peer) =>
            container =>
            {
                container.Bind<IRuntimeSettings>().FromInstance(new RuntimeSettings(peer, ValidationLevel)).AsSingle();
                if (peer == PeerType.Server)
                    container.BindInstance(new RoomSettings(ValidationLevel, Guid.NewGuid(), Array.Empty<RoomBot>())).AsSingle();
            };

        public DiContainer Project()
        {
            var container = new DiContainer(true);
            container.Settings = new ZenjectSettings(ValidationErrorResponses.Throw, RootResolveMethods.All);
            container.DefaultParent = _holder.transform;
            ZenjectManagersInstaller.Install(container);
            container.Bind(typeof(ProjectKernel), typeof(MonoKernel)).To<ProjectKernel>().FromNewComponentOn(_holder).AsSingle();
            container.Bind<SceneContextRegistry>().AsSingle();
            container.BindInterfacesTo<NetworkHolder>().AsSingle();
            container.Bind<NextLaunch>().AsSingle();
            container.Bind<SessionScenes>().AsSingle();
            container.Bind<GameRestarter>().AsSingle();

            foreach (var installer in SettingsInstallers())
            {
                container.Inject(installer);
                installer.InstallBindings();
            }

            return container;
        }

        public SceneContext Scene(string path, DiContainer parent) =>
            Scene(path, parent, _ => { });

        public SceneContext Scene(string path, DiContainer parent, Action<DiContainer> session)
        {
            var scene = EditorSceneManager.OpenPreviewScene(path);
            _scenes.Add(scene);
            var context = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<SceneContext>(true)).Single();
            context.ParentContractNames = Array.Empty<string>();
            context.ParentNewObjectsUnderSceneContext = true;
            SceneContext.ParentContainers = new[] { parent };
            SceneContext.ExtraBindingsInstallMethod = session;
            if (context.Installers.Any(installer => installer is PrefabHandlersInstaller))
            {
                context.Installers = context.Installers.Where(installer => installer is not PrefabHandlersInstaller).ToList();
                SceneContext.ExtraBindingsInstallMethod = container =>
                {
                    session(container);
                    BindPrefabHandlers(container);
                };
            }

            try
            {
                context.Install();
            }
            finally
            {
                SceneContext.ParentContainers = null;
                SceneContext.ExtraBindingsInstallMethod = null;
            }

            return context;
        }

        public void UseGameNetworkManager()
        {
            if (_networkManager != null)
                return;

            _networkManager = PrefabUtility.LoadPrefabContents(NetworkManagerPrefab);
            var manager = _networkManager.GetComponent<NetworkManager>();
            manager.NetworkConfig.Prefabs.Initialize();
            manager.SetSingleton();
        }

        public void Dispose()
        {
            foreach (var scene in _scenes)
                EditorSceneManager.ClosePreviewScene(scene);

            _scenes.Clear();
            if (_networkManager != null)
                PrefabUtility.UnloadPrefabContents(_networkManager);

            _networkManager = null;
            Object.DestroyImmediate(_holder);
        }

        private static void BindPrefabHandlers(DiContainer container)
        {
            container.Bind<IPrefabPools>().FromMethodUntyped(_ => null);
            container.Bind<NetworkPrefabHandlerFactory>().AsSingle().WithArguments(Lifetime.Eternal);
            container.BindInterfacesTo<NetworkPrefabRegistrationBus>().AsSingle().WithArguments(Lifetime.Eternal, NetworkManager.Singleton);
        }
    }
}

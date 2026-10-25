using System;
using System.Collections.Generic;
using System.Linq;
using Bw.Entities.Network;
using Bw.Entities.Pool.GameObjects;
using Bw.Injection.Prefabs;
using Bw.UseCases;
using Bw.UseCases.Spawning.Network;
using JetBrains.Lifetimes;
using Setup;
using Unity.Netcode;
using UnityEditor;
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
        public const string SetupScene = "Assets/Resources/Scenes/GameSetupScene.unity";
        public const string NetworkScene = "Assets/Resources/Scenes/Network.unity";
        public const string NetworkManagerPrefab = "Assets/Game/Prefabs/Network/Network.prefab";

        private readonly List<Scene> _scenes = new();
        private readonly GameObject _holder;

        private GameObject _networkManager;

        public ValidationContainers()
        {
            _holder = EditorUtility.CreateGameObjectWithHideFlags("Validation", HideFlags.HideAndDontSave);
        }

        public static IEnumerable<string> Levels() =>
            EditorBuildSettings.scenes.Select(scene => scene.path).Where(path => path != SetupScene && path != NetworkScene);

        public static RuntimeSettings SettingsOf(PeerType peer)
        {
            var settings = new RuntimeSettings();
            settings.Initialize(peer);
            return settings;
        }

        public DiContainer Project(IRuntimeSettings settings)
        {
            var container = new DiContainer(true);
            container.Settings = new ZenjectSettings(ValidationErrorResponses.Throw, RootResolveMethods.All);
            container.DefaultParent = _holder.transform;
            ZenjectManagersInstaller.Install(container);
            container.Bind(typeof(ProjectKernel), typeof(MonoKernel)).To<ProjectKernel>().FromNewComponentOn(_holder).AsSingle();
            container.Bind<SceneContextRegistry>().AsSingle();
            container.Bind<IRuntimeSettings>().FromInstance(settings);
            container.BindInterfacesTo<NetworkHolder>().AsSingle();
            if (settings.CurrentPeerType == PeerType.Server)
                container.BindInterfacesTo<CharacterRegistry>().AsSingle();

            foreach (var installer in AssetDatabase.LoadAssetAtPath<GameObject>(ProjectContextPrefab).GetComponent<ProjectContext>().ScriptableObjectInstallers)
            {
                container.Inject(installer);
                installer.InstallBindings();
            }

            return container;
        }

        public SceneContext Scene(string path, DiContainer parent)
        {
            var scene = EditorSceneManager.OpenPreviewScene(path);
            _scenes.Add(scene);
            var context = scene.GetRootGameObjects().SelectMany(root => root.GetComponentsInChildren<SceneContext>(true)).Single();
            context.ParentContractNames = Array.Empty<string>();
            context.ParentNewObjectsUnderSceneContext = true;
            SceneContext.ParentContainers = new[] { parent };
            if (context.Installers.Any(installer => installer is PrefabHandlersInstaller))
            {
                context.Installers = context.Installers.Where(installer => installer is not PrefabHandlersInstaller).ToList();
                SceneContext.ExtraBindingsInstallMethod = BindPrefabHandlers;
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
            container.BindInterfacesTo<NetworkPrefabRegistrationBus>().AsSingle().WithArguments(Lifetime.Eternal, NetworkManager.Singleton);
        }
    }
}

using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;

namespace Setup.Launch
{
    public sealed class SetupSceneLoader : MonoBehaviour
    {
        public const string SetupScene = "GameSetupScene";

        private void Start() =>
            Addressables.LoadSceneAsync(SetupScene, LoadSceneMode.Single);
    }
}

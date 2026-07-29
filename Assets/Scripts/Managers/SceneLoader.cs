using System;
using System.Collections;
using ToyRepairShop.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ToyRepairShop.Managers
{
    /// <summary>
    /// Central entry point for all scene loading. Wraps SceneManager so no
    /// other code calls it directly, and guards against overlapping requests.
    /// </summary>
    public sealed class SceneLoader : MonoSingleton<SceneLoader>
    {
        /// <summary>Raised with a 0-1 progress value while an async load is in flight.</summary>
        public event Action<float> LoadProgressChanged;

        /// <summary>Raised once a scene has finished loading.</summary>
        public event Action<string> SceneLoaded;

        public bool IsLoading { get; private set; }

        /// <summary>Loads a scene synchronously, replacing all currently loaded scenes.</summary>
        public void LoadScene(string sceneName)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"SceneLoader: ignoring LoadScene('{sceneName}') while a load is already in progress.");
                return;
            }

            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            SceneLoaded?.Invoke(sceneName);
        }

        /// <summary>Loads a scene asynchronously, replacing all currently loaded scenes.</summary>
        public void LoadSceneAsync(string sceneName)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"SceneLoader: ignoring LoadSceneAsync('{sceneName}') while a load is already in progress.");
                return;
            }

            StartCoroutine(LoadSceneRoutine(sceneName, LoadSceneMode.Single));
        }

        /// <summary>Loads a scene additively, on top of the currently loaded scenes.</summary>
        public void LoadSceneAdditive(string sceneName)
        {
            if (IsLoading)
            {
                Debug.LogWarning($"SceneLoader: ignoring LoadSceneAdditive('{sceneName}') while a load is already in progress.");
                return;
            }

            StartCoroutine(LoadSceneRoutine(sceneName, LoadSceneMode.Additive));
        }

        /// <summary>Unloads a previously additive-loaded scene.</summary>
        public void UnloadScene(string sceneName)
        {
            SceneManager.UnloadSceneAsync(sceneName);
        }

        private IEnumerator LoadSceneRoutine(string sceneName, LoadSceneMode mode)
        {
            IsLoading = true;

            AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName, mode);
            while (operation != null && !operation.isDone)
            {
                LoadProgressChanged?.Invoke(operation.progress);
                yield return null;
            }

            IsLoading = false;
            SceneLoaded?.Invoke(sceneName);
        }
    }
}

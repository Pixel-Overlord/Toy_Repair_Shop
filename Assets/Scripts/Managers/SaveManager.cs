using System.Collections;
using ToyRepairShop.Core;
using ToyRepairShop.Data;
using UnityEngine;

namespace ToyRepairShop.Managers
{
    /// <summary>
    /// Generic key/value save system. Currently backed by PlayerPrefs through
    /// IPersistenceProvider; swap the provider to change storage without
    /// touching any call site.
    /// </summary>
    public sealed class SaveManager : MonoSingleton<SaveManager>
    {
        [SerializeField] private bool _autoSaveEnabled = true;
        [SerializeField] private float _autoSaveIntervalSeconds = 60f;

        private IPersistenceProvider _provider;

        protected override void Initialize()
        {
            _provider = new PlayerPrefsPersistenceProvider();

            if (_autoSaveEnabled)
            {
                StartCoroutine(AutoSaveRoutine());
            }
        }

        public void SaveInt(string key, int value) => _provider.SetInt(key, value);
        public int LoadInt(string key, int defaultValue = 0) => _provider.GetInt(key, defaultValue);

        public void SaveFloat(string key, float value) => _provider.SetFloat(key, value);
        public float LoadFloat(string key, float defaultValue = 0f) => _provider.GetFloat(key, defaultValue);

        public void SaveString(string key, string value) => _provider.SetString(key, value);
        public string LoadString(string key, string defaultValue = "") => _provider.GetString(key, defaultValue);

        public void SaveBool(string key, bool value) => _provider.SetBool(key, value);
        public bool LoadBool(string key, bool defaultValue = false) => _provider.GetBool(key, defaultValue);

        /// <summary>Removes a single saved key.</summary>
        public void DeleteKey(string key) => _provider.DeleteKey(key);

        /// <summary>Removes every saved key. Use with care.</summary>
        public void DeleteAll() => _provider.DeleteAll();

        /// <summary>Flushes pending writes to disk immediately.</summary>
        public void Save() => _provider.Save();

        private IEnumerator AutoSaveRoutine()
        {
            var wait = new WaitForSeconds(_autoSaveIntervalSeconds);
            while (true)
            {
                yield return wait;
                Save();
            }
        }

        private void OnApplicationPause(bool isPaused)
        {
            if (isPaused)
            {
                Save();
            }
        }

        private void OnApplicationQuit()
        {
            Save();
        }
    }
}

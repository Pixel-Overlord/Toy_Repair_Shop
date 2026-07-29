using UnityEngine;

namespace ToyRepairShop.Data
{
    /// <summary>
    /// PlayerPrefs-backed implementation of IPersistenceProvider.
    /// PlayerPrefs has no native bool type, so booleans are stored as 0/1.
    /// </summary>
    public sealed class PlayerPrefsPersistenceProvider : IPersistenceProvider
    {
        public void SetInt(string key, int value) => PlayerPrefs.SetInt(key, value);
        public int GetInt(string key, int defaultValue) => PlayerPrefs.GetInt(key, defaultValue);

        public void SetFloat(string key, float value) => PlayerPrefs.SetFloat(key, value);
        public float GetFloat(string key, float defaultValue) => PlayerPrefs.GetFloat(key, defaultValue);

        public void SetString(string key, string value) => PlayerPrefs.SetString(key, value);
        public string GetString(string key, string defaultValue) => PlayerPrefs.GetString(key, defaultValue);

        public void SetBool(string key, bool value) => PlayerPrefs.SetInt(key, value ? 1 : 0);
        public bool GetBool(string key, bool defaultValue) => PlayerPrefs.GetInt(key, defaultValue ? 1 : 0) == 1;

        public bool HasKey(string key) => PlayerPrefs.HasKey(key);
        public void DeleteKey(string key) => PlayerPrefs.DeleteKey(key);
        public void DeleteAll() => PlayerPrefs.DeleteAll();
        public void Save() => PlayerPrefs.Save();
    }
}

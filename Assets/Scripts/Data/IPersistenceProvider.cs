namespace ToyRepairShop.Data
{
    /// <summary>
    /// Abstraction over a key/value persistence backend. SaveManager depends
    /// on this interface rather than PlayerPrefs directly, so the storage
    /// backend can be replaced later without changing any call site.
    /// </summary>
    public interface IPersistenceProvider
    {
        void SetInt(string key, int value);
        int GetInt(string key, int defaultValue);

        void SetFloat(string key, float value);
        float GetFloat(string key, float defaultValue);

        void SetString(string key, string value);
        string GetString(string key, string defaultValue);

        void SetBool(string key, bool value);
        bool GetBool(string key, bool defaultValue);

        bool HasKey(string key);
        void DeleteKey(string key);
        void DeleteAll();
        void Save();
    }
}

using UnityEngine;

namespace ToyRepairShop.Core
{
    /// <summary>
    /// Base class for MonoBehaviours that must exist as a single persistent
    /// instance across scene loads. Duplicate instances destroy themselves.
    /// </summary>
    public abstract class MonoSingleton<T> : MonoBehaviour where T : MonoSingleton<T>
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = (T)this;
            DontDestroyOnLoad(gameObject);
            Initialize();
        }

        /// <summary>
        /// Called once, immediately after this instance becomes the active
        /// singleton. Override this instead of Awake to set up state.
        /// </summary>
        protected virtual void Initialize()
        {
        }
    }
}

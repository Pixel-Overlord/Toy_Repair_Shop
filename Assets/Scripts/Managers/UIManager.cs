using System.Collections.Generic;
using ToyRepairShop.Core;
using UnityEngine;

namespace ToyRepairShop.Managers
{
    /// <summary>
    /// Generic UI navigation: switching between registered full screens and
    /// showing/hiding a single popup on top of them. Holds no game-specific UI.
    /// </summary>
    public sealed class UIManager : MonoSingleton<UIManager>
    {
        private readonly Dictionary<string, GameObject> _screens = new Dictionary<string, GameObject>();
        private GameObject _activeScreen;
        private GameObject _activePopup;

        /// <summary>Registers a screen under an id so it can later be shown via ShowScreen.</summary>
        public void RegisterScreen(string screenId, GameObject screen)
        {
            if (string.IsNullOrEmpty(screenId) || screen == null)
            {
                return;
            }

            _screens[screenId] = screen;
            screen.SetActive(false);
        }

        /// <summary>Deactivates the current screen and activates the one registered under screenId.</summary>
        public void ShowScreen(string screenId)
        {
            if (!_screens.TryGetValue(screenId, out GameObject screen))
            {
                Debug.LogWarning($"UIManager: no screen registered under id '{screenId}'.");
                return;
            }

            if (_activeScreen != null)
            {
                _activeScreen.SetActive(false);
            }

            _activeScreen = screen;
            _activeScreen.SetActive(true);
        }

        /// <summary>Shows a popup GameObject, hiding any popup already showing.</summary>
        public void ShowPopup(GameObject popup)
        {
            if (popup == null)
            {
                return;
            }

            if (_activePopup != null && _activePopup != popup)
            {
                _activePopup.SetActive(false);
            }

            _activePopup = popup;
            _activePopup.SetActive(true);
        }

        /// <summary>Hides the currently visible popup, if any.</summary>
        public void HideCurrentPopup()
        {
            if (_activePopup == null)
            {
                return;
            }

            _activePopup.SetActive(false);
            _activePopup = null;
        }
    }
}

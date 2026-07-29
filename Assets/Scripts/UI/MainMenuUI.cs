using ToyRepairShop.Data;
using ToyRepairShop.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ToyRepairShop.UI
{
    /// <summary>
    /// Wires up the three Main Menu buttons. Contains no gameplay logic -
    /// it only triggers scene loads and toggles the settings placeholder panel.
    /// </summary>
    public sealed class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private GameObject _settingsPanel;

        private void Awake()
        {
#if !UNITY_EDITOR
            if (_quitButton != null)
            {
                _quitButton.gameObject.SetActive(false);
            }
#endif
        }

        private void OnEnable()
        {
            if (_playButton != null) _playButton.onClick.AddListener(OnPlayClicked);
            if (_settingsButton != null) _settingsButton.onClick.AddListener(OnSettingsClicked);
            if (_quitButton != null) _quitButton.onClick.AddListener(OnQuitClicked);
        }

        private void OnDisable()
        {
            if (_playButton != null) _playButton.onClick.RemoveListener(OnPlayClicked);
            if (_settingsButton != null) _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (_quitButton != null) _quitButton.onClick.RemoveListener(OnQuitClicked);
        }

        private void OnPlayClicked()
        {
            SceneLoader.Instance.LoadSceneAsync(SceneNames.Workshop);
        }

        private void OnSettingsClicked()
        {
            UIManager.Instance.ShowPopup(_settingsPanel);
        }

        private void OnQuitClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}

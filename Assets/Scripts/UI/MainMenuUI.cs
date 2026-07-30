using ToyRepairShop.Data;
using ToyRepairShop.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace ToyRepairShop.UI
{
    /// <summary>
    /// Wires up the Main Menu's buttons and popups. Contains no gameplay
    /// logic - it only triggers scene loads and toggles the Settings/Help
    /// placeholder panels.
    /// </summary>
    public sealed class MainMenuUI : MonoBehaviour
    {
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private GameObject _settingsPanel;
        [SerializeField] private Button _settingsCloseButton;

        [Header("Help")]
        [SerializeField] private Button _helpButton;
        [SerializeField] private GameObject _helpPanel;
        [SerializeField] private Button _helpCloseButton;

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
            if (_settingsCloseButton != null) _settingsCloseButton.onClick.AddListener(OnCloseCurrentPopupClicked);
            if (_helpButton != null) _helpButton.onClick.AddListener(OnHelpClicked);
            if (_helpCloseButton != null) _helpCloseButton.onClick.AddListener(OnCloseCurrentPopupClicked);
        }

        private void OnDisable()
        {
            if (_playButton != null) _playButton.onClick.RemoveListener(OnPlayClicked);
            if (_settingsButton != null) _settingsButton.onClick.RemoveListener(OnSettingsClicked);
            if (_quitButton != null) _quitButton.onClick.RemoveListener(OnQuitClicked);
            if (_settingsCloseButton != null) _settingsCloseButton.onClick.RemoveListener(OnCloseCurrentPopupClicked);
            if (_helpButton != null) _helpButton.onClick.RemoveListener(OnHelpClicked);
            if (_helpCloseButton != null) _helpCloseButton.onClick.RemoveListener(OnCloseCurrentPopupClicked);
        }

        private void OnPlayClicked()
        {
            SceneLoader.Instance.LoadSceneAsync(SceneNames.Workshop);
        }

        private void OnSettingsClicked()
        {
            UIManager.Instance.ShowPopup(_settingsPanel);
        }

        private void OnHelpClicked()
        {
            UIManager.Instance.ShowPopup(_helpPanel);
        }

        private void OnCloseCurrentPopupClicked()
        {
            UIManager.Instance.HideCurrentPopup();
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

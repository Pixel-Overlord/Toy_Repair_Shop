using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ToyRepairShop.UI
{
    /// <summary>
    /// Reward popup shown when a toy finishes repair: coins earned, a
    /// "Repair Completed" message, and a Continue button the player must
    /// tap to proceed. Coins shown here are session-only - this view
    /// never touches SaveManager.
    /// </summary>
    public sealed class RewardPopupView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _rewardText;
        [SerializeField] private TMP_Text _completedText;
        [SerializeField] private Button _continueButton;

        private Action _onContinue;

        private void Awake()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }

            if (_completedText != null)
            {
                _completedText.text = "Repair Completed!";
            }

            if (_continueButton != null)
            {
                _continueButton.onClick.AddListener(HandleContinueClicked);
            }
        }

        private void OnDestroy()
        {
            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveListener(HandleContinueClicked);
            }
        }

        /// <summary>Shows the popup with the given coin amount. onContinue fires once the player taps Continue.</summary>
        public void Show(int coins, Action onContinue)
        {
            if (_root == null)
            {
                return;
            }

            if (_rewardText != null)
            {
                _rewardText.text = $"+{coins} Coins";
            }

            _onContinue = onContinue;
            _root.SetActive(true);
        }

        private void HandleContinueClicked()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }

            Action callback = _onContinue;
            _onContinue = null;
            callback?.Invoke();
        }
    }
}

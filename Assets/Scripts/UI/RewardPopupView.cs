using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ToyRepairShop.UI
{
    /// <summary>
    /// Reward popup shown when a toy finishes repair: a personalized
    /// "Congratulations for saving me" message, coins earned, and a
    /// Continue button the player must tap to proceed. Coins shown here
    /// are session-only - this view never touches SaveManager.
    /// </summary>
    public sealed class RewardPopupView : MonoBehaviour
    {
        private static readonly int ShowTrigger = Animator.StringToHash("Show");

        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _rewardText;
        [SerializeField] private TMP_Text _completedText;
        [SerializeField] private Button _continueButton;

        [Header("Entrance Animator (optional)")]
        [SerializeField, Tooltip("Fires 'Show' each time Show() is called. Leave unassigned until you have a clip to play.")]
        private Animator _animator;

        private Action _onContinue;

        private void Awake()
        {
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

        /// <summary>Shows the popup for the given toy and coin amount. onContinue fires once the player taps Continue.</summary>
        public void Show(string toyName, int coins, Action onContinue)
        {
            if (_root == null)
            {
                return;
            }

            if (_completedText != null)
            {
                _completedText.text = $"Thanks for saving me buddy!";
            }

            if (_rewardText != null)
            {
                _rewardText.text = $"+{coins} Coins";
            }

            _onContinue = onContinue;
            _root.SetActive(true);
            _animator?.SetTrigger(ShowTrigger);
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

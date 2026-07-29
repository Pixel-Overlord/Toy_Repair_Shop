using System.Collections;
using TMPro;
using UnityEngine;

namespace ToyRepairShop.UI
{
    /// <summary>
    /// Placeholder "+Coins" popup shown when a toy finishes repair.
    /// Auto-hides after a short delay. Coins shown here are session-only -
    /// this view never touches SaveManager.
    /// </summary>
    public sealed class RewardPopupView : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TMP_Text _rewardText;
        [SerializeField] private float _autoHideSeconds = 2f;

        private Coroutine _hideRoutine;

        private void Awake()
        {
            if (_root != null)
            {
                _root.SetActive(false);
            }
        }

        /// <summary>Shows the popup with the given coin amount, auto-hiding after a delay.</summary>
        public void Show(int coins)
        {
            if (_root == null)
            {
                return;
            }

            if (_rewardText != null)
            {
                _rewardText.text = $"+{coins} Coins";
            }

            _root.SetActive(true);

            if (_hideRoutine != null)
            {
                StopCoroutine(_hideRoutine);
            }

            _hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(_autoHideSeconds);
            _root.SetActive(false);
            _hideRoutine = null;
        }
    }
}

using TMPro;
using UnityEngine;

namespace ToyRepairShop.UI
{
    /// <summary>
    /// Displays the live repair HUD: toy name, the contextual "what to do
    /// next" hint, and the progress bar. Delegates the fill bar to
    /// ProgressBarView instead of duplicating that logic. Purely display -
    /// no repair rules live here.
    /// </summary>
    public sealed class RepairHUDView : MonoBehaviour
    {
        [SerializeField] private TMP_Text _toyNameText;
        [SerializeField] private ProgressBarView _progressBarView;

        [SerializeField, Tooltip("Contextual 'what to do next' hint. Left blank until a repair session actually starts.")]
        private TMP_Text _instructionText;

        public void SetToyName(string toyName)
        {
            if (_toyNameText != null)
            {
                _toyNameText.text = toyName;
            }
        }

        public void SetProgress(float normalizedProgress)
        {
            _progressBarView?.SetProgress(normalizedProgress);
        }

        public void SetProgressVisible(bool visible)
        {
            _progressBarView?.SetVisible(visible);
        }

        public void PlayStepSuccessPulse()
        {
            _progressBarView?.PlaySuccessPulse();
        }

        /// <summary>Sets the contextual "what to do next" hint. Pass an empty string to clear it.</summary>
        public void SetInstruction(string message)
        {
            if (_instructionText != null)
            {
                _instructionText.text = message ?? string.Empty;
            }
        }
    }
}

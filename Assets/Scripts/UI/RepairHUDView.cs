using TMPro;
using UnityEngine;

namespace ToyRepairShop.UI
{
    /// <summary>
    /// Displays the live repair HUD: toy name, current/next step, current
    /// tool, and remaining step count. Delegates the fill bar to
    /// ProgressBarView instead of duplicating that logic. Purely display -
    /// no repair rules live here.
    /// </summary>
    public sealed class RepairHUDView : MonoBehaviour
    {
        private const string NoneLabel = "-";

        [SerializeField] private TMP_Text _toyNameText;
        [SerializeField] private TMP_Text _currentStepText;
        [SerializeField] private TMP_Text _nextStepText;
        [SerializeField] private TMP_Text _currentToolText;
        [SerializeField] private TMP_Text _remainingStepsText;
        [SerializeField] private ProgressBarView _progressBarView;

        public void SetToyName(string toyName)
        {
            if (_toyNameText != null)
            {
                _toyNameText.text = toyName;
            }
        }

        public void SetCurrentStep(string stepName)
        {
            if (_currentStepText != null)
            {
                _currentStepText.text = string.IsNullOrEmpty(stepName) ? NoneLabel : stepName;
            }
        }

        public void SetNextStep(string stepName)
        {
            if (_nextStepText != null)
            {
                _nextStepText.text = string.IsNullOrEmpty(stepName) ? NoneLabel : stepName;
            }
        }

        public void SetCurrentTool(string toolName)
        {
            if (_currentToolText != null)
            {
                _currentToolText.text = string.IsNullOrEmpty(toolName) ? NoneLabel : toolName;
            }
        }

        public void SetRemainingSteps(int count)
        {
            if (_remainingStepsText != null)
            {
                _remainingStepsText.text = count.ToString();
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
    }
}

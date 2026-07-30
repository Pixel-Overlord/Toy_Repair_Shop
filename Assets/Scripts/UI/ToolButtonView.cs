using System;
using System.Collections;
using ToyRepairShop.Data.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace ToyRepairShop.UI
{
    /// <summary>
    /// A single toolbar button. Reports taps via an event; the "selected"
    /// scale feedback and incorrect-tool shake are applied externally by
    /// ToolbarView, so this view has no gameplay or tool-selection
    /// knowledge of its own.
    /// </summary>
    public sealed class ToolButtonView : MonoBehaviour
    {
        private static readonly int SelectedTrigger = Animator.StringToHash("Selected");

        [SerializeField] private ToolType _tool;
        [SerializeField] private Button _button;
        [SerializeField] private RectTransform _visualRoot;
        [SerializeField] private float _selectedScale = 1.15f;

        [Header("Correct Selection Animator (optional)")]
        [SerializeField, Tooltip("Fires 'Selected' when this tool is correctly chosen for the current step. Leave unassigned until you have a clip to play.")]
        private Animator _animator;

        [Header("Incorrect Tool Feedback")]
        [SerializeField] private float _shakeDuration = 0.3f;
        [SerializeField] private float _shakeMagnitude = 12f;

        /// <summary>Raised with this button's tool type when clicked.</summary>
        public event Action<ToolType> Clicked;

        public ToolType Tool => _tool;

        private Coroutine _shakeRoutine;

        private void OnEnable()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(HandleClick);
            }
        }

        private void OnDisable()
        {
            if (_button != null)
            {
                _button.onClick.RemoveListener(HandleClick);
            }
        }

        private void HandleClick()
        {
            Clicked?.Invoke(_tool);
        }

        /// <summary>Applies (or clears) the selected-scale visual feedback.</summary>
        public void SetSelected(bool isSelected)
        {
            if (_visualRoot == null)
            {
                return;
            }

            _visualRoot.localScale = isSelected ? Vector3.one * _selectedScale : Vector3.one;
        }

        /// <summary>Fires the Selected animator trigger to indicate this tool was correctly chosen.</summary>
        public void PlaySelectedFeedback()
        {
            _animator?.SetTrigger(SelectedTrigger);
        }

        /// <summary>Plays a brief shake to indicate this tool was tapped incorrectly.</summary>
        public void PlayIncorrectFeedback()
        {
            if (_visualRoot == null)
            {
                return;
            }

            if (_shakeRoutine != null)
            {
                StopCoroutine(_shakeRoutine);
            }

            _shakeRoutine = StartCoroutine(ShakeRoutine());
        }

        private IEnumerator ShakeRoutine()
        {
            Vector3 originalPosition = _visualRoot.anchoredPosition3D;
            float elapsed = 0f;

            while (elapsed < _shakeDuration)
            {
                elapsed += Time.deltaTime;
                float damper = 1f - elapsed / _shakeDuration;
                float offsetX = Mathf.Sin(elapsed * 40f) * _shakeMagnitude * damper;
                _visualRoot.anchoredPosition3D = originalPosition + new Vector3(offsetX, 0f, 0f);
                yield return null;
            }

            _visualRoot.anchoredPosition3D = originalPosition;
            _shakeRoutine = null;
        }
    }
}

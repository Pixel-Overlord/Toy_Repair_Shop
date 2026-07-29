using System;
using ToyRepairShop.Data.Enums;
using UnityEngine;
using UnityEngine.UI;

namespace ToyRepairShop.UI
{
    /// <summary>
    /// A single toolbar button. Reports taps via an event; the "selected"
    /// scale feedback is applied externally by ToolbarView, so this view
    /// has no gameplay or tool-selection knowledge of its own.
    /// </summary>
    public sealed class ToolButtonView : MonoBehaviour
    {
        [SerializeField] private ToolType _tool;
        [SerializeField] private Button _button;
        [SerializeField] private RectTransform _visualRoot;
        [SerializeField] private float _selectedScale = 1.15f;

        /// <summary>Raised with this button's tool type when clicked.</summary>
        public event Action<ToolType> Clicked;

        public ToolType Tool => _tool;

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
    }
}

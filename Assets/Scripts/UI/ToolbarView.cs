using System;
using System.Collections.Generic;
using ToyRepairShop.Data.Enums;
using ToyRepairShop.Gameplay.Controllers;
using ToyRepairShop.Managers;
using UnityEngine;

namespace ToyRepairShop.UI
{
    /// <summary>
    /// Wires a set of ToolButtonView instances to a ToolManager. Purely a
    /// UI adapter - no repair/gameplay rules live here. Tap validity
    /// (whether the tapped tool is actually correct for the current
    /// repair step) is decided by RepairController; this view only
    /// reports taps via ToolTapped and plays feedback when told to.
    /// </summary>
    public sealed class ToolbarView : MonoBehaviour
    {
        [SerializeField] private List<ToolButtonView> _buttons = new List<ToolButtonView>();

        [SerializeField, Tooltip("Optional placeholder SFX played when a tool is selected.")]
        private AudioClip _toolSelectedSfx;

        /// <summary>Raised whenever any tool button is tapped, regardless of whether it turns out to be correct.</summary>
        public event Action<ToolType> ToolTapped;

        private ToolManager _selection;

        /// <summary>Wires this toolbar to the given tool manager. Called by the scene's composition root.</summary>
        public void Initialize(ToolManager selection)
        {
            _selection = selection;

            foreach (ToolButtonView button in _buttons)
            {
                if (button != null)
                {
                    button.Clicked += HandleButtonClicked;
                }
            }

            _selection.ToolSelected += HandleToolSelected;
        }

        private void OnDestroy()
        {
            foreach (ToolButtonView button in _buttons)
            {
                if (button != null)
                {
                    button.Clicked -= HandleButtonClicked;
                }
            }

            if (_selection != null)
            {
                _selection.ToolSelected -= HandleToolSelected;
            }
        }

        private void HandleButtonClicked(ToolType tool)
        {
            AudioManager.Instance?.PlaySFX(_toolSelectedSfx);
            _selection.SelectTool(tool);
            ToolTapped?.Invoke(tool);
        }

        private void HandleToolSelected(ToolType? selectedTool)
        {
            foreach (ToolButtonView button in _buttons)
            {
                if (button != null)
                {
                    button.SetSelected(selectedTool.HasValue && button.Tool == selectedTool.Value);
                }
            }
        }

        /// <summary>Plays the incorrect-tool shake on the button for the given tool, if present.</summary>
        public void PlayIncorrectFeedback(ToolType tool)
        {
            foreach (ToolButtonView button in _buttons)
            {
                if (button != null && button.Tool == tool)
                {
                    button.PlayIncorrectFeedback();
                }
            }
        }
    }
}

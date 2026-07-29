using System.Collections.Generic;
using ToyRepairShop.Data.Enums;
using ToyRepairShop.Gameplay.Controllers;
using ToyRepairShop.Managers;
using UnityEngine;

namespace ToyRepairShop.UI
{
    /// <summary>
    /// Wires a set of ToolButtonView instances to a ToolSelectionController.
    /// Purely a UI adapter - no repair/gameplay rules live here.
    /// </summary>
    public sealed class ToolbarView : MonoBehaviour
    {
        [SerializeField] private List<ToolButtonView> _buttons = new List<ToolButtonView>();

        [SerializeField, Tooltip("Optional placeholder SFX played when a tool is selected.")]
        private AudioClip _toolSelectedSfx;

        private ToolSelectionController _selection;

        /// <summary>Wires this toolbar to the given selection controller. Called by the scene's composition root.</summary>
        public void Initialize(ToolSelectionController selection)
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
    }
}

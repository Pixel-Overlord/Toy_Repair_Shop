using System;
using System.Collections.Generic;
using ToyRepairShop.Data.Enums;

namespace ToyRepairShop.Gameplay.Controllers
{
    /// <summary>
    /// Owns which single tool is currently selected and which tools are
    /// available to select. Kept generic and data-agnostic so future
    /// tools plug in without changing this class - only one tool is ever
    /// selected at a time. Renamed and expanded from Stage 4's
    /// ToolSelectionController to add availability tracking (Stage 5).
    /// </summary>
    public sealed class ToolManager
    {
        /// <summary>Raised whenever the selected tool changes, including selection being cleared (null).</summary>
        public event Action<ToolType?> ToolSelected;

        public ToolType? SelectedTool { get; private set; }

        private readonly HashSet<ToolType> _unavailableTools = new HashSet<ToolType>();

        public bool IsAvailable(ToolType tool)
        {
            return !_unavailableTools.Contains(tool);
        }

        /// <summary>Marks a tool as available or unavailable (e.g. locked). Unavailable tools cannot be selected.</summary>
        public void SetAvailable(ToolType tool, bool available)
        {
            if (available)
            {
                _unavailableTools.Remove(tool);
            }
            else
            {
                _unavailableTools.Add(tool);
            }
        }

        /// <summary>Selects a tool. Returns false without changing anything if the tool is unavailable.</summary>
        public bool SelectTool(ToolType tool)
        {
            if (!IsAvailable(tool))
            {
                return false;
            }

            if (SelectedTool == tool)
            {
                return true;
            }

            SelectedTool = tool;
            ToolSelected?.Invoke(tool);
            return true;
        }

        public void ClearSelection()
        {
            if (SelectedTool == null)
            {
                return;
            }

            SelectedTool = null;
            ToolSelected?.Invoke(null);
        }
    }
}

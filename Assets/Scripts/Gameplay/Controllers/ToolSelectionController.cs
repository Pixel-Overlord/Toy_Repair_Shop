using System;
using ToyRepairShop.Data.Enums;

namespace ToyRepairShop.Gameplay.Controllers
{
    /// <summary>
    /// Tracks which single tool is currently selected. Kept generic and
    /// data-agnostic so future tools (needle, brush, paint roller, ...)
    /// plug in without changing this class - only one tool is ever
    /// selected at a time.
    /// </summary>
    public sealed class ToolSelectionController
    {
        /// <summary>Raised whenever the selected tool changes, including selection being cleared (null).</summary>
        public event Action<ToolType?> ToolSelected;

        public ToolType? SelectedTool { get; private set; }

        public void SelectTool(ToolType tool)
        {
            if (SelectedTool == tool)
            {
                return;
            }

            SelectedTool = tool;
            ToolSelected?.Invoke(tool);
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

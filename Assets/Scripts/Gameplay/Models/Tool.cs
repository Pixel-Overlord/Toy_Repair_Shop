using ToyRepairShop.Data.ScriptableObjects;

namespace ToyRepairShop.Gameplay.Models
{
    /// <summary>
    /// Runtime state for a tool: pairs static ToolData with the player's
    /// current unlock state.
    /// </summary>
    public sealed class Tool
    {
        public ToolData Data { get; }
        public bool IsUnlocked { get; private set; }

        public Tool(ToolData data, bool isUnlocked)
        {
            Data = data;
            IsUnlocked = isUnlocked;
        }

        public void Unlock()
        {
            IsUnlocked = true;
        }
    }
}

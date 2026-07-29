using ToyRepairShop.Data.ScriptableObjects;

namespace ToyRepairShop.Gameplay.Repositories
{
    /// <summary>
    /// Read access to the tool catalogue. Consumers depend on this
    /// interface instead of holding a direct reference to a ToolDatabase asset.
    /// </summary>
    public interface IToolRepository
    {
        ToolData GetById(string toolId);
    }
}

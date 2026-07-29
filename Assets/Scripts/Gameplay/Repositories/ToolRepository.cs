using ToyRepairShop.Data.ScriptableObjects;

namespace ToyRepairShop.Gameplay.Repositories
{
    /// <summary>
    /// Default IToolRepository, backed by a ToolDatabase asset injected at
    /// construction time.
    /// </summary>
    public sealed class ToolRepository : IToolRepository
    {
        private readonly ToolDatabase _database;

        public ToolRepository(ToolDatabase database)
        {
            _database = database;
        }

        public ToolData GetById(string toolId) => _database.GetToolById(toolId);
    }
}

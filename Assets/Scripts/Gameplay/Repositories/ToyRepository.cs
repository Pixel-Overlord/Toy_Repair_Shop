using System.Collections.Generic;
using ToyRepairShop.Data.ScriptableObjects;

namespace ToyRepairShop.Gameplay.Repositories
{
    /// <summary>
    /// Default IToyRepository, backed by a ToyDatabase asset injected at
    /// construction time.
    /// </summary>
    public sealed class ToyRepository : IToyRepository
    {
        private readonly ToyDatabase _database;

        public ToyRepository(ToyDatabase database)
        {
            _database = database;
        }

        public ToyData GetById(string toyId) => _database.GetToyById(toyId);
        public IReadOnlyList<ToyData> GetAllUnlocked(int playerLevel) => _database.GetAllUnlocked(playerLevel);
        public ToyData GetRandom() => _database.GetRandomToy();
    }
}

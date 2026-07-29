using System.Collections.Generic;
using ToyRepairShop.Data.ScriptableObjects;

namespace ToyRepairShop.Gameplay.Repositories
{
    /// <summary>
    /// Read access to the toy catalogue. Consumers depend on this interface
    /// instead of holding a direct reference to a ToyDatabase asset.
    /// </summary>
    public interface IToyRepository
    {
        ToyData GetById(string toyId);
        IReadOnlyList<ToyData> GetAllUnlocked(int playerLevel);
        ToyData GetRandom();
    }
}

using ToyRepairShop.Data.ScriptableObjects;
using ToyRepairShop.Gameplay.Models;

namespace ToyRepairShop.Gameplay.Factories
{
    /// <summary>
    /// Builds runtime Toy instances from static ToyData. Pure C# - no
    /// MonoBehaviour, no GameObjects - so it can be constructed and tested
    /// without a scene.
    /// </summary>
    public sealed class ToyFactory
    {
        /// <summary>Creates a new runtime Toy wrapping the given data, or null if data is null.</summary>
        public Toy Create(ToyData data)
        {
            return data == null ? null : new Toy(data);
        }
    }
}

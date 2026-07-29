using ToyRepairShop.Data.Enums;

namespace ToyRepairShop.Gameplay.Behaviours
{
    /// <summary>
    /// Looks up the IRepairBehaviour strategy for a given RepairType.
    /// RepairController depends on this abstraction instead of
    /// constructing or branching on concrete behaviour types.
    /// </summary>
    public interface IRepairBehaviourRegistry
    {
        /// <summary>Returns the behaviour registered for repairType, or throws if none is registered.</summary>
        IRepairBehaviour GetBehaviour(RepairType repairType);
    }
}

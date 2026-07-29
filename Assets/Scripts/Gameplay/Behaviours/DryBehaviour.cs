using ToyRepairShop.Data.Enums;

namespace ToyRepairShop.Gameplay.Behaviours
{
    /// <summary>
    /// Placeholder: reuses the generic drag-to-fill mechanic. A future stage can override UpdateProgress for a distinct drying gesture.
    /// </summary>
    public sealed class DryBehaviour : RepairBehaviourBase
    {
        public override RepairType RepairType => RepairType.Dry;
    }
}

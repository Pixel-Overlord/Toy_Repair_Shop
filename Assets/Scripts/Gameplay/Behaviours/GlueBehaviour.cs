using ToyRepairShop.Data.Enums;

namespace ToyRepairShop.Gameplay.Behaviours
{
    /// <summary>
    /// Placeholder: reuses the generic drag-to-fill mechanic. A future stage can override UpdateProgress for a distinct hold-and-press gesture.
    /// </summary>
    public sealed class GlueBehaviour : RepairBehaviourBase
    {
        public override RepairType RepairType => RepairType.Glue;
    }
}

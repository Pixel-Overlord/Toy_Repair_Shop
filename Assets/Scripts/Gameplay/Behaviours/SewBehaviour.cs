using ToyRepairShop.Data.Enums;

namespace ToyRepairShop.Gameplay.Behaviours
{
    /// <summary>
    /// Placeholder: reuses the generic drag-to-fill mechanic. A future stage can override UpdateProgress for a distinct stitching gesture.
    /// </summary>
    public sealed class SewBehaviour : RepairBehaviourBase
    {
        public override RepairType RepairType => RepairType.Sew;
    }
}

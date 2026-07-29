using ToyRepairShop.Data.Enums;

namespace ToyRepairShop.Gameplay.Behaviours
{
    /// <summary>
    /// Fully implemented: uses the base progress mechanic as-is, driven by RepairDragInteraction. This is the one behaviour Stage 4 proved end to end.
    /// </summary>
    public sealed class WashBehaviour : RepairBehaviourBase
    {
        public override RepairType RepairType => RepairType.Wash;
    }
}

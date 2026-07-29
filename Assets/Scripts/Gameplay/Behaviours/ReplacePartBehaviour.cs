using ToyRepairShop.Data.Enums;

namespace ToyRepairShop.Gameplay.Behaviours
{
    /// <summary>
    /// Placeholder: reuses the generic drag-to-fill mechanic. A future stage can override UpdateProgress for a distinct drag-and-drop part swap.
    /// </summary>
    public sealed class ReplacePartBehaviour : RepairBehaviourBase
    {
        public override RepairType RepairType => RepairType.ReplacePart;
    }
}
